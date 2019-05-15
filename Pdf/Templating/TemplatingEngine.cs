using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pdf.Storage.Pdf.Templating;
using Stubble.Core;
using Stubble.Core.Builders;
using Stubble.Extensions.JsonNet;

namespace Pdf.Storage.Pdf
{
    public class TemplatingEngine
    {
        private readonly StubbleVisitorRenderer _stubble = new StubbleBuilder().Configure(settings => settings.AddJsonNet()).Build();
        private readonly BarcodeLib.Barcode _barcode = new BarcodeLib.Barcode();
        private readonly Regex _barcodeTranslatorPattern = new Regex(@"\[translate\:barcode(\:)?(.*)?\](.*)");

        public string Render(string template, JObject data)
        {
            ApplyBarcodeTranslatorsIfAny(data);

            return _stubble.Render(template, data);
        }

        private void ApplyBarcodeTranslatorsIfAny(JObject data)
        {
            foreach (var property in data.Properties())
            {
                var translatorMatch = _barcodeTranslatorPattern.Match(property.Value.ToString());

                if (translatorMatch.Success)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        var options = GetBarcodeOptions(translatorMatch);

                        var barcode = _barcode.Encode(
                            ResolveBarcodeType(options.Type),
                            translatorMatch.Groups[3].Value,
                            ColorTranslator.FromHtml(options.ForegroundColor),
                            ColorTranslator.FromHtml(options.BackgroundColor),
                            options.Width,
                            options.Height);

                        barcode.Save(memoryStream, ImageFormat.Png);

                        property.Value = $"data:image/png;base64,{Convert.ToBase64String(memoryStream.ToArray())}";
                    }
                }
            }
        }

        private static BarcodeOptions GetBarcodeOptions(Match translatorMatch)
        {
            var options = new BarcodeOptions();

            if (!string.IsNullOrEmpty(translatorMatch.Groups[2].Value))
            {
                options = JsonConvert.DeserializeObject<BarcodeOptions>(translatorMatch.Groups[2].Value);
            }

            return options;
        }

        private BarcodeLib.TYPE ResolveBarcodeType(string type)
        {
            switch(type)
            {
                case "code128":
                    return BarcodeLib.TYPE.CODE128;
                case "ean13":
                    return BarcodeLib.TYPE.EAN13;
                case "ean8":
                    return BarcodeLib.TYPE.EAN8;
                case "upca":
                    return BarcodeLib.TYPE.UPCA;
                case "upce":
                    return BarcodeLib.TYPE.UPCE;
                case "itf14":
                    return BarcodeLib.TYPE.ITF14;
                case "code39":
                    return BarcodeLib.TYPE.CODE39;
                default:
                    return BarcodeLib.TYPE.CODE128;
            }
        }
    }
}