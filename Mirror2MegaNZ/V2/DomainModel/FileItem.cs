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
            if (!CanCreate(file, baseFolderPath))
            {
                throw new InvalidOperationException("The FileItem is not child of the base folder");
            }

            Name = file.Name;
            Type = ItemType.File;

            baseFolderPath = baseFolderPath.TrimEnd(new[] { '\\' });
            Path = BuildRelativePath(file.FullName, baseFolderPath);
            Size = file.Length;
            LastModified = new DateTime(file.LastWriteTimeUtc.Year,
                file.LastWriteTimeUtc.Month,
                file.LastWriteTimeUtc.Day,
                file.LastWriteTimeUtc.Hour,
                file.LastWriteTimeUtc.Minute,
                file.LastWriteTimeUtc.Second);
        }

        public FileItem(IDirectoryInfo folder, string baseFolderPath)
        {
            if( !CanCreate(folder, baseFolderPath))
            {
                throw new InvalidOperationException("The FileItem is not child of the base folder");
            }
            
            Type = ItemType.Folder;
            baseFolderPath = baseFolderPath.TrimEnd(new[] { '\\' });
            Path = BuildRelativePath(folder.FullName, baseFolderPath);
            Name = folder.Name ?? string.Empty;     // We need this to be able to compare with a Root Node from MegaNz
            Size = 0;
        }

        private bool CanCreate(IFileInfo file, string baseFolderPath)
        {
            // Can create only if the file is a child of the base folder
            return file.FullName.Contains(baseFolderPath);
        }

        private bool CanCreate(IDirectoryInfo folder, string baseFolderPath)
        {
            // Can create only if the folder is a child of the base folder
            return folder.FullName.Contains(baseFolderPath);
        }

        private string BuildRelativePath(string absolutePath, string basePath)
        {
            var relativePath = absolutePath.Substring(basePath.Length);
            return string.IsNullOrEmpty(relativePath) ? @"\" : relativePath;
        }
    }
}
