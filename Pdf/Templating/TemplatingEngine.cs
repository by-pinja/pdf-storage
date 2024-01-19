using Newtonsoft.Json.Linq;
using Stubble.Core;
using Stubble.Core.Builders;
using Stubble.Extensions.JsonNet;

namespace Pdf.Storage.Pdf
{
    public class TemplatingEngine
    {
        private readonly StubbleVisitorRenderer _stubble = new StubbleBuilder().Configure(settings => settings.AddJsonNet()).Build();

        public string Render(string template, JObject data)
        {
            return _stubble.Render(template, data);
        }
    }
}
