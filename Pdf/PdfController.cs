using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
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
        private readonly TemplatingEngine _templatingEngine;
        private readonly IErrorPages _errorPages;
        private readonly IMqMessages _mqMessages;

        public PdfController(
            PdfDataContext context,
            IStorage pdfStorage,
            Uris uris,
            IHangfireQueue backgroundJob,
            TemplatingEngine templatingEngine,
            IErrorPages errorPages,
            IMqMessages mqMessages)
        {
            _context = context;
            _pdfStorage = pdfStorage;
            _uris = uris;
            _backgroundJobs = backgroundJob;
            _templatingEngine = templatingEngine;
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
        /// <param name="groupId">Group id is any string user like to provide that can be used to group PDF:s in some meaningfull manner.</param>
        /// <param name="request"></param>
        [Authorize(AuthenticationSchemes = "ApiKey")]
        [HttpPost("/v1/pdf/{groupId}/")]
        public ActionResult<IEnumerable<NewPdfResponse>> AddNewPdf([Required][FromRoute] string groupId, [FromBody] NewPdfRequest request)
        {
            if (!request.RowData.Any())
                return BadRequest("Expected to get atleast one 'rowData' element, but got none.");

            var responses = request.RowData.Select(row =>
            {
                var entity = _context.PdfFiles.Add(new PdfEntity(groupId, PdfType.Pdf) { Options = request.Options }).Entity;
                var templatedRow = TemplateUtils.MergeBaseTemplatingWithRows(request.BaseData, row);
                PersistParsedHtmlTemplateOfPdfDocument(entity, request.Html, templatedRow);

                _context.SaveChanges();

                entity.HangfireJobId =
                    _backgroundJobs.Enqueue<IPdfQueue>(que => que.CreatePdf(entity.Id));

                _context.SaveChanges();

                var pdfUri = _uris.PdfUri(groupId, entity.FileId);
                var htmlUri = _uris.HtmlUri(groupId, entity.FileId);

                return new NewPdfResponse(entity.FileId, entity.GroupId, pdfUri, htmlUri, row);
            });

            return Accepted(responses.ToList());
        }

        private void PersistParsedHtmlTemplateOfPdfDocument(PdfEntity entity, string html, JObject templateData)
        {
            var templatedHtml = _templatingEngine.Render(html, templateData);
            templatedHtml = TemplateUtils.AddWaitForAllPageElementsFixToHtml(templatedHtml);
            _pdfStorage.AddOrReplace(new StorageData(new StorageFileId(entity, "html"), Encoding.UTF8.GetBytes(templatedHtml)));
        }

        /// <summary>
        /// Generated PDF uri
        /// </summary>
        /// <remarks>
        /// This api serves generated PDF files and HTML data in it's raw for if requested.
        /// </remarks>
        [HttpGet("/v1/pdf/{groupId}/{pdfId}.{extension}")]
        public IActionResult Get([FromRoute] string groupId, [FromRoute] string pdfId, [FromRoute] string extension, [FromQuery] bool noCount)
        {
            if (extension != "html" && extension != "pdf")
            {
                return BadRequest("Only extensions 'pdf' and 'html' are supported.");
            }

            var pdfEntity = _context.PdfFiles.FirstOrDefault(x => x.GroupId == groupId && x.FileId == pdfId);

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

            var pdfOrHtml = _pdfStorage.Get(new StorageFileId(groupId, pdfId, extension));

            if (!noCount)
            {
                pdfEntity.Usage.Add(new PdfOpenedEntity());
                _context.SaveChanges();
                _mqMessages.PdfOpened(groupId, pdfId);
            }

            return new FileStreamResult(new MemoryStream(pdfOrHtml.Data), pdfOrHtml.ContentType);
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
        public IActionResult GetPdfHead([FromRoute] string groupId, [FromRoute] string pdfId)
        {
            var processedFileFound = _context
                .PdfFiles
                .AsNoTracking()
                .Any(x =>
                    x.GroupId == groupId &&
                    x.FileId == pdfId &&
                    x.Processed);

            if (!processedFileFound)
                return NotFound();

            return Ok();
        }

        /// <summary>
        /// Delete single PDF
        /// </summary>
        [HttpDelete("/v1/pdf/{groupId}/{pdfId}.{_}")]
        public IActionResult RemoveSinglePdf([FromRoute] string groupId, [FromRoute] string pdfId)
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
            var pdfEntity = _context.PdfFiles.FirstOrDefault(x => x.GroupId == groupId && x.FileId == pdfId);

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
