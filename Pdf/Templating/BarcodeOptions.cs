using System.Drawing;

namespace Pdf.Storage.Pdf.Templating
{
    public class BarcodeOptions
    {
        public string Type { get; set; } = "code128";
        public int Width { get; set; } = 290;
        public int Height { get; set; } = 120;
        public bool IncludeText { get; set; } = false;
        public string ForegroundColor { get; set; } = "#ffffff";
        public string BackgroundColor { get; set; } = "#000000";
    }
}