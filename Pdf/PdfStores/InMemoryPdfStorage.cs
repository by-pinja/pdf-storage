using System.Collections.Generic;
using Pdf.Storage.Pdf;
using Pdf.Storage.Pdf.PdfStores;

namespace Pdf.Storage.Hangfire
{
    public class InMemoryPdfStorage : IStorage
    {
        private readonly Dictionary<string, StorageData> _localStore = new Dictionary<string, StorageData>();

        public void AddOrReplace(StorageData storageData)
        {
            if (_localStore.ContainsKey(GetKey(storageData.StorageFileId)))
                _localStore.Remove(GetKey(storageData.StorageFileId));

            _localStore.Add(GetKey(storageData.StorageFileId), storageData);
        }

        public StorageData Get(StorageFileId storageFileId)
        {
            return _localStore[GetKey(storageFileId)];
        }

        public void Remove(StorageFileId storageFileId)
        {
            if (_localStore.ContainsKey(GetKey(storageFileId)))
                _localStore.Remove(GetKey(storageFileId));
        }

        private static string GetKey(StorageFileId storageFileId)
        {
            return $"{storageFileId.Group}_{storageFileId.Id}.{storageFileId.Extension}";
        }
    }
}
