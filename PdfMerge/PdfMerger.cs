using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Pdf.Storage.Data;
using Pdf.Storage.Pdf;
using Pdf.Storage.Test;

namespace Pdf.Storage.PdfMerge
{
    public class PdfMerger : IPdfMerger
    {
        private readonly IHostingEnvironment _env;
        private readonly IPdfStorage _pdfStorage;
        private readonly PdfDataContext _context;
        private readonly ILogger<PdfMerger> _logger;

        public PdfMerger(IHostingEnvironment env, IPdfStorage pdfStorage, PdfDataContext context, ILogger<PdfMerger> logger)
        {
            _env = env;
            _pdfStorage = pdfStorage;
            _context = context;
            _logger = logger;
        }

        public void MergePdf(string groupId, string fileId, MergeRequest[] requests)
        {
            var temp = ResolveTemporaryDirectory();

            _logger.LogInformation($"Using temporary folder: {temp}");

            try
            {
                var mergedFile = _context.PdfFiles.Single(x => x.GroupId == groupId && x.FileId == fileId);

                var pdfs = requests
                    .Select(x => _pdfStorage.GetPdf(x.Group, x.PdfId))
                    .Select(x => new
                    {
                        TempFile = Path.Combine($@"{temp}", $"{x.Id}.pdf"),
                        x.Data
                    }).ToList();

                pdfs.ForEach(x => File.WriteAllBytes(x.TempFile, x.Data));

                var mergedPdf = MergeFiles(temp, pdfs.Select(x => x.TempFile));

                _pdfStorage.AddOrReplacePdf(new StoredPdf(groupId, fileId, mergedPdf));

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
            var p = GetCorrectProcessForSystem(tempPath, tempFiles);

            p.Start();
            p.WaitForExit();

            var consoleOutput = p.StandardOutput.ReadToEnd();

            if (!string.IsNullOrEmpty(consoleOutput))
                _logger.LogInformation("Console returned: " + p.StandardOutput.ReadToEnd());

            var consoleErrors = p.StandardError.ReadToEnd();

            if (!string.IsNullOrEmpty(consoleOutput))
                throw new InvalidOperationException("Console threw error message:" + consoleErrors);

            return File.ReadAllBytes(Path.Combine(tempPath, "concat.pdf")).ToArray();
        }

        private Process GetCorrectProcessForSystem(string tempPath, IEnumerable<string> tempFiles)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return CreateProcess(
                    workingDir: tempPath, 
                    fileName: $@"{_env.ContentRootPath}\PdfMerge\PdfTkForWin\pdftk.exe", 
                    arguments: tempFiles.Aggregate("", (a, b) => a + " " + b) + " cat output concat.pdf");
            }

            return CreateProcess(
                workingDir: tempPath,
                fileName: "/bin/bash",
                arguments: "-c \"/usr/bin/pdftk" + tempFiles.Aggregate("", (a, b) => a + " " + b) + $" cat output {tempPath}/concat.pdf\"");
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
