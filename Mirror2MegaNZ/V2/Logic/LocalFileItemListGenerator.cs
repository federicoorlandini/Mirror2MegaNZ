using Mirror2MegaNZ.V2.DomainModel;
using System.Collections.Generic;
using SystemInterface.IO;

namespace Mirror2MegaNZ.V2.Logic
{
    /// <summary>
    /// This class generate a list of FileItem from the local file system
    /// </summary>
    internal class LocalFileItemListGenerator
    {
        public List<FileItem> Generate(IDirectoryInfo root, string basePath)
        {
            var fileItemList = new List<FileItem>();

            var localRootFileItem = new FileItem(root, basePath);
            fileItemList.Add(localRootFileItem);

            IFileInfo[] files = root.GetFiles();
            foreach (var file in files)
            {
                var fileItem = new FileItem(file, basePath);
                fileItemList.Add(fileItem);
            }

            IDirectoryInfo[] directories = root.GetDirectories();
            foreach (var directory in directories)
            {
                var subList = Generate(directory, basePath);
                fileItemList.AddRange(subList);
            }

            return fileItemList;
        }
    }
}
