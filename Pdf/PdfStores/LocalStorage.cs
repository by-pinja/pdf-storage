using System.Collections.Generic;
using System.IO.Abstractions;
using Microsoft.Extensions.Options;
using Pdf.Storage.Pdf;
using Pdf.Storage.Pdf.Config;
using Pdf.Storage.Pdf.PdfStores;

namespace Pdf.Storage.Hangfire
{
    public class LocalStorage : IStorage
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _folder;

        public LocalStorage(IFileSystem fs, IOptions<LocalStorageConfig> options)
        {
            _fileSystem = fs;

            _folder = options.Value.Folder ?? "/tmp/";
            if (_folder[^1] != '/')
                _folder += "/";
        }

        public void AddOrReplace(StorageData storageData)
        {
            if (_fileSystem.File.Exists(GetPath(storageData.StorageFileId)))
                _fileSystem.File.Delete(GetPath(storageData.StorageFileId));

            _fileSystem.File.WriteAllBytes(GetPath(storageData.StorageFileId), storageData.Data);
        }

        public StorageData Get(StorageFileId storageFileId)
        {
            if (!_fileSystem.File.Exists(GetPath(storageFileId)))
                return null;

            var bytes = _fileSystem.File.ReadAllBytes(GetPath(storageFileId));
            var result = new StorageData(storageFileId, bytes);

            return result;
        }

        public void Remove(StorageFileId storageFileId)
        {
            if (_fileSystem.File.Exists(GetPath(storageFileId)))
                _fileSystem.File.Delete(GetPath(storageFileId));
        }

        private string GetKey(StorageFileId storageFileId)
        {
            return $"{storageFileId.Group}_{storageFileId.Id}.{storageFileId.Extension}";
        }

        private string GetPath(StorageFileId storageFileId)
        {
            return $"{_folder}{GetKey(storageFileId)}";
        }
    }
}
