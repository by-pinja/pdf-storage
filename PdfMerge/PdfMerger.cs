using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Pdf.Storage.Data;
using Pdf.Storage.Pdf;
using Pdf.Storage.Hangfire;
using Pdf.Storage.Util;
using Pdf.Storage.Pdf.PdfStores;

namespace Pdf.Storage.PdfMerge
{
    public class PdfMerger : IPdfMerger
    {
        private readonly IHostingEnvironment _env;
        private readonly IStorage _pdfStorage;
        private readonly PdfDataContext _context;
        private readonly ILogger<PdfMerger> _logger;

        public PdfMerger(IHostingEnvironment env, IStorage pdfStorage, PdfDataContext context, ILogger<PdfMerger> logger)
        {
            _env = env;
            _pdfStorage = pdfStorage;
            _context = context;
            _logger = logger;
        }

        [Queue(HangfireConstants.HighPriorityQueue)]
        [AutomaticRetry(DelaysInSeconds = new [] { 10, 10, 15, 15, 15, 15, 30, 30, 30, 30, 30, 30, 30, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 120, 120, 600, 600 })]
        public void MergePdf(StorageFileId storageIdForMergedPdf, string[] pdfIds)
        {
            Debug.Assert(storageIdForMergedPdf.Extension == "pdf");

            var temp = ResolveTemporaryDirectory();

            _logger.LogInformation($"Using temporary folder: {temp}");

            try
            {
                var mergedFile = _context.PdfFiles.Single(x => x.GroupId == storageIdForMergedPdf.Group && x.FileId == storageIdForMergedPdf.Id);

                var pdfEntities = _context.PdfFiles
                    .Where(x => x.GroupId == storageIdForMergedPdf.Group)
                    .Where(x => pdfIds.Any(id => x.FileId == id))
                    .ToList();

                if(pdfEntities.Any(x => !x.Processed))
                    throw new InvalidOperationException(
                        $"Tried to merge files that are not ready, non ready pdf list is '{pdfEntities.Where(x => !x.Processed).Select(x => x.FileId).Aggregate("", (a, b) => $"{a}, {b}").Trim(',')}'");

                var pdfs = pdfIds
                    .Select(id => _pdfStorage.Get(new StorageFileId(storageIdForMergedPdf.Group, id, "pdf")))
                    .Select(pdf => (tempFile: Path.Combine($@"{temp}", $"{pdf.StorageFileId.Id}.pdf"), data: pdf.Data)).ToList();

                pdfs.ForEach(x => File.WriteAllBytes(x.tempFile, x.data));

                var mergedPdf = MergeFiles(temp, pdfs.Select(x => x.tempFile));

                _pdfStorage.AddOrReplace(new StorageData(storageIdForMergedPdf, mergedPdf));

                mergedFile.Processed = true;

                _context.SaveChanges();
            }
            finally
            {
                _logger.LogInformation($"Removing temporary folder: {temp}");
                Directory.Delete(temp, true);
            }
        }

        private byte[] MergeFiles(string tempPath, IEnumerable<string> tempFiles)
        {
            using var p = GetCorrectProcessForSystem(tempPath, tempFiles);

            p.Start();
            p.WaitForExit(60 * 1000);

            _logger.LogInformation("StdOut: " + p.StandardOutput.ReadToEnd());
            _logger.LogInformation("StdError: " + p.StandardError.ReadToEnd());

            return File.ReadAllBytes(Path.Combine(tempPath, "concat.pdf")).ToArray();
        }

        private Process GetCorrectProcessForSystem(string tempPath, IEnumerable<string> tempFiles)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return CreateProcess(
                    workingDir: tempPath,
                    fileName: $@"{_env.ContentRootPath}\PdfMerge\QPdfForWin\qpdf.exe",
                    arguments: tempFiles.Aggregate("--empty --pages ", (a, b) => $"{a} {b} 1-z") + " -- concat.pdf");
            }

            return CreateProcess(
                workingDir: tempPath,
                fileName: "qpdf",
                arguments:  tempFiles.Aggregate("--empty --pages ", (a, b) => $"{a} {b} 1-z") + $" -- {tempPath}/concat.pdf");
        }

        private Process CreateProcess(string workingDir, string fileName, string arguments)
        {
            _logger.LogInformation($"Running '{fileName} {arguments}'");

            return new Process
            {
                StartInfo =
                    {
                        WorkingDirectory = workingDir,
                        FileName = fileName,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
            };
        }

        private string ResolveTemporaryDirectory()
        {
            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);
            return tempFolder;
        }
    }
}
