using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Pdf.Storage.Pdf
{
    public class PdfConvert : IPdfConvert
    {
        private readonly ILogger<PdfConvert> _logger;
        private readonly IOptions<CommonConfig> _settings;

        public PdfConvert(ILogger<PdfConvert> logger, IOptions<CommonConfig> settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public byte[] CreatePdfFromHtml(string html, JObject options)
        {
            var tempDir = ResolveTemporaryDirectory();

            try
            {
                File.WriteAllText(Path.Combine(tempDir, "source.html"), html);

                var data = GeneratePdf(tempDir);

                return data;
            }
            finally
            {
                _logger.LogInformation($"Removing temporary folder: {tempDir}");
                Directory.Delete(tempDir, true);
            }
        }

        private byte[] GeneratePdf(string tempPath)
        {
            var p = GetCorrectProcessForSystem(tempPath);

                p.Start();

                var processExitedNicely = p.WaitForExit(30 * 1000);

                _logger.LogInformation("StdOut: " + p.StandardOutput.ReadToEnd());
                _logger.LogInformation("StdError: " + p.StandardError.ReadToEnd());

                if(!processExitedNicely)
                {
                    _logger.LogError($"Process {p.ProcessName} didn't exit nicely.");
                    p.Kill();
                    throw new InvalidOperationException("Failed to generate pdf.");
                }

                return File.ReadAllBytes(Path.Combine(tempPath, "output.pdf")).ToArray();
        }

        private Process GetCorrectProcessForSystem(string tempPath)
        {
            var args = $"--headless --disable-gpu --use-mock-keychain --no-sandbox --hide-scrollbars --print-to-pdf={Path.Combine(tempPath, "output.pdf")} {Path.Combine(tempPath, "source.html")}";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return CreateProcess(
                    workingDir: tempPath,
                    fileName: _settings.Value.WindowsChromePath
                        ?? throw new InvalidOperationException("Chrome path for windows not set."),
                    arguments: args);
            }

            return CreateProcess(
                workingDir: tempPath,
                fileName: "chromium-browser",
                arguments: args);
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
