﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pdf.Storage.Data;
using Pdf.Storage.Hangfire;
using Pdf.Storage.Mq;
using Pdf.Storage.Pdf.CustomPages;
using Pdf.Storage.Pdf.Dto;
using Pdf.Storage.Pdf.PdfStores;
using Pdf.Storage.Util;

namespace Pdf.Storage.Pdf
{
    public class PdfController : Controller
    {
        private readonly PdfDataContext _context;
        private readonly IStorage _pdfStorage;
        private readonly Uris _uris;
        private readonly IHangfireQueue _backgroundJobs;
        private readonly IErrorPages _errorPages;
        private readonly IMqMessages _mqMessages;

        public PdfController(
            PdfDataContext context,
            IStorage pdfStorage,
            Uris uris,
            IHangfireQueue backgroundJob,
            IErrorPages errorPages,
            IMqMessages mqMessages)
        {
            _context = context;
            _pdfStorage = pdfStorage;
            _uris = uris;
            _backgroundJobs = backgroundJob;
            _errorPages = errorPages;
            _mqMessages = mqMessages;
        }

        [Authorize(AuthenticationSchemes = "ApiKey")]
        [HttpPost("/v1/pdf/{groupId}/")]
        public ActionResult<IEnumerable<NewPdfResponse>> AddNewPdf([Required] string groupId, [FromBody] NewPdfRequest request)
        {
            var responses = request.RowData.ToList().Select(row =>
            {
                var entity = _context.PdfFiles.Add(new PdfEntity(groupId, PdfType.Pdf)).Entity;

                var rawData = _context.RawData.Add(
                    new PdfRawDataEntity(entity.Id,
                        request.Html,
                        TemplateDataUtils.GetTemplateData(request.BaseData, row),
                        request.Options)).Entity;

                _context.SaveChanges();

                entity.HangfireJobId =
                    _backgroundJobs.Enqueue<IPdfQueue>(que => que.CreatePdf(entity.Id));

                _context.SaveChanges();

                var pdfUri = _uris.PdfUri(groupId, entity.FileId);
                var htmlUri = _uris.HtmlUri(groupId, entity.FileId);

                return new NewPdfResponse(entity.FileId, entity.GroupId, pdfUri, htmlUri, row);
            });

            return StatusCode(202, responses.ToList());
        }

        [HttpGet("/v1/pdf/{groupId}/{pdfId}.{extension}")]
        public IActionResult Get(string groupId, string pdfId, string extension, [FromQuery] bool noCount)
        {
            if(extension != "html" && extension != "pdf")
            {
                return BadRequest("Only extensions 'pdf' and 'html' are supported.");
            }

            var pdfEntity = _context.PdfFiles.SingleOrDefault(x => x.GroupId == groupId && x.FileId == pdfId);

            if (pdfEntity == null)
            {
                return _errorPages.PdfNotFoundResponse();
            }

            if (pdfEntity.Removed)
            {
                return _errorPages.PdfRemovedResponse();
            }

            if (!pdfEntity.Processed)
            {
                if (pdfEntity.IsValidForHighPriority())
                {
                    pdfEntity.MarkAsHighPriority(
                        _backgroundJobs.EnqueueWithHighPriority<IPdfQueue>(que => que.CreatePdf(pdfEntity.Id), originalJobId: pdfEntity.HangfireJobId));

                    _context.SaveChanges();
                }

                return _errorPages.PdfIsStillProcessingResponse();
            }

            var pdf = _pdfStorage.Get(new StorageFileId(groupId, pdfId, extension));

            if (!noCount)
            {
                pdfEntity.Usage.Add(new PdfOpenedEntity());
                _context.SaveChanges();
                _mqMessages.PdfOpened(groupId, pdfId);
            }

            return new FileStreamResult(new MemoryStream(pdf.Data), pdf.ContentType);
        }

        [HttpHead("/v1/pdf/{groupId}/{pdfId}.{extension}")]
        public IActionResult GetPdfHead(string groupId, string pdfId, string extension)
        {
            var pdfEntity = _context.PdfFiles.SingleOrDefault(x => x.GroupId == groupId && x.FileId == pdfId);

            if (pdfEntity == null)
            {
                return NotFound();
            }

            if (!pdfEntity.Processed)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpDelete("/v1/pdf/{groupId}/{pdfId}.{_}")]
        public IActionResult RemoveSinglePdf(string groupId, string pdfId)
        {
            if (!RemovePdf(groupId, pdfId))
                return NotFound();

            return Ok();
        }

        [HttpDelete("/v1/pdfs/")]
        public IActionResult RemoveMultiplePdfs([FromBody][Required] IEnumerable<PdfDeleteRequest> request)
        {
            var removedItems =
                    request
                        .Select(x =>
                        {
                            var removed = RemovePdf(x.GroupId, x.PdfId);
                            return new { Request = x, Removed = removed };
                        })
                        .ToList()
                        .Where(x => x.Removed)
                        .Select(x => x.Request);

            return Ok(removedItems);
        }

        private bool RemovePdf(string groupId, string pdfId)
        {
            var pdfEntity = _context.PdfFiles.SingleOrDefault(x => x.GroupId == groupId && x.FileId == pdfId);

            if (pdfEntity == null)
                return false;

            if (pdfEntity.Removed)
                return true;

            // This delay solves folloing problem, if pdfs are added, merged and then removed instantly, merge requires these
            // binaries on its background jobs and deleting them during those routines creates complicated scenarios.
            // To avoid that scenario this delay is added, this makes pretty sure that all pdfs are generated before delete.
            _backgroundJobs.Schedule<IStorage>(storage => storage.Remove(new StorageFileId(pdfEntity, "pdf")), TimeSpan.FromDays(1));
            _backgroundJobs.Schedule<IStorage>(storage => storage.Remove(new StorageFileId(pdfEntity, "html")), TimeSpan.FromDays(1));

            pdfEntity.Removed = true;
            _context.SaveChanges();

            return true;
        }
    }
}
