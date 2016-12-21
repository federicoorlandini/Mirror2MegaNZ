using System;
using SystemInterface.IO;

namespace Mirror2MegaNZ.V2.DomainModel
{
    internal class FileItem : IItem
    {
        public ItemType Type { get; private set; }
        public string Path { get; private set; }
        public long Size { get; private set; }
        public DateTime? LastModified { get; private set; }

        public string Name
        {
            get
            {
                if ( Path == @"\" )
                {
                    return @"\";
                }
                return GetNameFromPath(Path);
            }
        }

        private string GetNameFromPath(string path)
        {
            path = path.TrimEnd('\\');
            var lastBackSlashIndex = path.LastIndexOf('\\');
            return path.Substring(lastBackSlashIndex + 1);
        }

        public FileItem(ItemType type, string path, long size, DateTime? lastModified = null)
        {
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

            Type = ItemType.File;

            Path = BuildRelativePath(file.FullName, baseFolderPath); // The file must NOT HAVE a final backslash
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
            Path = BuildRelativePath(folder.FullName, baseFolderPath) + "\\";    // The folder must have a final backslash
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
            basePath = basePath.TrimEnd('\\');
            absolutePath = absolutePath.TrimEnd('\\');
            var relativePath = absolutePath.Substring(basePath.Length);
            return relativePath;
        }
    }
}
