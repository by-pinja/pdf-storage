using System;
using System.Collections.Concurrent;

namespace Pdf.Storage.Pdf
{
    public class TemplateCacheService
    {
        private readonly ConcurrentDictionary<Guid, string> _cache;

        public TemplateCacheService()
        {
            _cache = new ConcurrentDictionary<Guid, string>();
        }

        public void Store(Guid pdfEntityId, string value)
        {
            _cache.TryAdd(pdfEntityId, value);
        }

        public string? Get(Guid pdfEntityId)
        {
            return _cache.TryGetValue(pdfEntityId, out var value) ? value : default;
        }

        public void Remove(Guid pdfEntityId)
        {
            _cache.TryRemove(pdfEntityId, out _);
        }
    }
}
