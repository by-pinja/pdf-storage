using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.Logging;

namespace Pdf.Storage.Pdf
{
    public class PdfConvert : IPdfConvert
    {
        private readonly ILogger<PdfConvert> _logger;

        public PdfConvert(ILogger<PdfConvert> logger)
        {
            _logger = logger;
        }

        public (byte[] data, string html) CreatePdfFromHtml(string html, object templateData, object options)
        {
            _logger.LogInformation("Generating pdf");

            var tempDir = ResolveTemporaryDirectory();

            File.WriteAllText(Path.Combine(tempDir, "source.html"), html);

            var data = GeneratePdf(tempDir);

            _logger.LogInformation("Pdf generated");

            return (data, html);
        }

        private byte[] GeneratePdf(string tempPath)
        {
            var p = GetCorrectProcessForSystem(tempPath);

            p.Start();
            p.WaitForExit(5*1000);

            // _logger.LogInformation("StdOut: " + p.StandardOutput.ReadToEnd());
            // _logger.LogInformation("StdError: " + p.StandardError.ReadToEnd());

            return File.ReadAllBytes(Path.Combine(tempPath, "output.pdf")).ToArray();
        }

        private Process GetCorrectProcessForSystem(string tempPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return CreateProcess(
                    workingDir: tempPath,
                    fileName: "chrome",
                    arguments: $"--headless --hide-scrollbars --print-to-pdf='{Path.Combine(tempPath, "output.pdf")}' '{Path.Combine(tempPath, "source.html")}'");
            }

            return CreateProcess(
                workingDir: tempPath,
                fileName: "chromium",
                arguments: "");
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
                        UseShellExecute = true,
                        // RedirectStandardOutput = true,
                        // RedirectStandardError = true
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
