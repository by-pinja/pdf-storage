using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Hosting;
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

        public PdfMerger(IHostingEnvironment env, IPdfStorage pdfStorage, PdfDataContext context)
        {
            _env = env;
            _pdfStorage = pdfStorage;
            _context = context;
        }

        public void MergePdf(string groupId, string fileId, MergeRequest[] requests)
        {
            var temp = ResolveTemporaryDirectory();

            try
            {
                var mergedFile = _context.PdfFiles.Single(x => x.GroupId == groupId && x.FileId == fileId);

                var pdfs = requests
                    .Select(x => _pdfStorage.GetPdf(x.Group, x.PdfId))
                    .Select(x => new
                    {
                        TempFile = $@"{temp}\{x.Id}.pdf",
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
                Directory.Delete(temp, true);
            }
        }

        private byte[] MergeFiles(string tempPath, IEnumerable<string> tempFiles)
        {
            Process p = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = tempPath,
                    FileName = ResolveStartupPathForPdfTk(),
                    Arguments = tempFiles.Aggregate("", (a,b) => a + " " + b) + " cat output concat.pdf",
                    UseShellExecute = false
                }
            };

            p.Start();
            p.WaitForExit();

            return File.ReadAllBytes(Path.Combine(tempPath, "concat.pdf"));
        }

        private string ResolveStartupPathForPdfTk()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $@"{_env.ContentRootPath}\PdfMerge\PdfTkForWin\pdftk.exe" : "/usr/bin/pdftk";
        }

        private string ResolveTemporaryDirectory()
        {
            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);
            return tempFolder;
        }
    }
}
