﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
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

        /// <summary>
        /// Create new PDF from template.
        /// </summary>
        /// <remarks>
        /// This is root of most operations in pdf storage. It will generate PDF and return constant resource URI where
        /// PDF will be available in future.
        ///
        /// All generator operations are asyncronous so it might take few seconds before user can get actual template.
        /// Until then in browser user will see message page that automatically shows PDF once it's ready.
        /// </remarks>
        /// <param name="groupId">Group id is is any string user like to provide that can be used to group PDF:s in some meaningfull manner.</param>
        /// <param name="request"></param>
        [Authorize(AuthenticationSchemes = "ApiKey")]
        [HttpPost("/v1/pdf/{groupId}/")]
        public ActionResult<IEnumerable<NewPdfResponse>> AddNewPdf([Required] string groupId, [FromBody] NewPdfRequest request)
        {
            if (!request.RowData.Any())
                return BadRequest("Expected to get attleast one 'rowData' element, but got none.");

            var responses = request.RowData.Select(row =>
            {
                var entity = _context.PdfFiles.Add(new PdfEntity(groupId, PdfType.Pdf)).Entity;

                var rawData = _context.RawData.Add(
                    new PdfRawDataEntity(entity.Id,
                        request.Html,
                        TemplateUtils.MergeBaseTemplatingWithRows(request.BaseData, row),
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

        /// <summary>
        /// Generated PDF uri
        /// </summary>
        /// <remarks>
        /// This api serves generated PDF files and HTML data in it's raw for if requested.
        /// </remarks>
        [HttpGet("/v1/pdf/{groupId}/{pdfId}.{extension}")]
        public IActionResult Get([FromQuery] string groupId, [FromQuery] string pdfId, [FromQuery] string extension, [FromQuery] bool noCount)
        {
            if (extension != "html" && extension != "pdf")
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

        /// <summary>
        /// Since generating PDF is asyncronous this HEAD can be used to poll if pdf is ready.
        /// </summary>
        /// <remarks>
        /// This is commonly used with server side integrations where some third part needs to know
        /// when PDF is ready. One example is printing service that automatically prints file once it's
        /// available.
        /// </remarks>
        [HttpHead("/v1/pdf/{groupId}/{pdfId}.{extension}")]
        public IActionResult GetPdfHead(string groupId, string pdfId)
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

        /// <summary>
        /// Delete single PDF
        /// </summary>
        [HttpDelete("/v1/pdf/{groupId}/{pdfId}.{_}")]
        public IActionResult RemoveSinglePdf(string groupId, string pdfId)
        {
            if (!RemovePdf(groupId, pdfId))
                return NotFound();

            return Ok();
        }

        /// <summary>
        /// Batch delete
        /// </summary>
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
