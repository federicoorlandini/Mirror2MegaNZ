using CG.Web.MegaApiClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemInterface.IO;
using SystemWrapper.IO;

namespace Mirror2MegaNZ.V2.DomainModel
{
    internal class FileItem : IItem
    {
        public string Name { get; private set; }
        public ItemType Type { get; private set; }
        public string Path { get; private set; }
        public long Size { get; private set; }
        public DateTime? LastModified { get; private set; }

        public FileItem(string name, ItemType type, string path, long size, DateTime? lastModified = null)
        {
            Name = name;
            Type = type;
            Path = path;
            Size = size;
            LastModified = lastModified;
        }

        public FileItem(IFileInfo file, string baseFolderPath)
        {
            Name = file.Name;
            Type = ItemType.File;

            baseFolderPath = baseFolderPath.TrimEnd(new[] { '\\' });
            Path = BuildRelativePath(file.FullName, baseFolderPath);
            Size = file.Length;
        }

        public FileItem(IDirectoryInfo folder, string baseFolderPath)
        {
            Name = string.Empty;
            Type = ItemType.Folder;

            baseFolderPath = baseFolderPath.TrimEnd(new[] { '\\' });
            Path = BuildRelativePath(folder.FullName, baseFolderPath);
            Size = 0;
        }

        private string BuildRelativePath(string absolutePath, string basePath)
        {
            return absolutePath.Substring(basePath.Length);
        }
    }
}
