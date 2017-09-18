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
using Pdf.Storage.Test;
using Pdf.Storage.Util;

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

        public void MergePdf(string groupId, string fileId, string[] pdfIds)
        {
            var temp = ResolveTemporaryDirectory();

            _logger.LogInformation($"Using temporary folder: {temp}");

            try
            {
                var mergedFile = _context.PdfFiles.Single(x => x.GroupId == groupId && x.FileId == fileId);

                // Its possible that pdf:s are just requested before merge, hangfire retry is very slow and with multiple distributed consumers
                // its very hard to control which is run first. For this reason this workaround is made.
                var pdfs = Retry.Func(() => pdfIds
                        .Select(id => _pdfStorage.GetPdf(groupId, id))
                        .Select(pdf => (tempFile: Path.Combine($@"{temp}", $"{pdf.Id}.pdf"), data: pdf.Data)).ToList(), 
                    retryInterval: TimeSpan.FromSeconds(10), maxAttemptCount: 4);

                pdfs.ToList().ForEach(x => File.WriteAllBytes(x.tempFile, x.data));

                var mergedPdf = MergeFiles(temp, pdfs.Select(x => x.tempFile));

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
