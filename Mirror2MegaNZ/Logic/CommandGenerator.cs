using Mirror2MegaNZ.DomainModel;
using Mirror2MegaNZ.DomainModel.Commands;
using System.Collections.Generic;
using System.Linq;

namespace Mirror2MegaNZ.Logic
{
    /// <summary>
    /// This is the class that synchronize the remote repository with
    /// the local repository
    /// </summary>
    internal class CommandGenerator
    {
        private readonly IEqualityComparer<IItem> _equalityComparer = new ItemComparer();
        public string LocalBasePath { get; private set;} 

        public CommandGenerator(string localbasePath)
        {
            LocalBasePath = localbasePath;
        }

        public List<ICommand> GenerateCommandList(List<FileItem> localItems, List<MegaNzItem> remoteItems)
        {
            // TODO - Federico: Not in use
            // We must visit all the tree and compute the path for each node
            //var megaNzNodeIdByPath = remoteItems.ToDictionary(item => item.Path, item => item.MegaNzId);

            // Generate the list of the file to be deleted from remote
            var commands = new List<ICommand>();
            var remoteFileItems = remoteItems.Where(item => item.Type == ItemType.File).ToArray();
            var localFileItems = localItems.Where(item => item.Type == ItemType.File).ToArray();
            var filesToDelete = remoteFileItems.Except(localFileItems, _equalityComparer).Cast<MegaNzItem>().ToArray();

            foreach (var file in filesToDelete)
            {
                commands.Add(new DeleteFileCommand {
                    PathToDelete = file.Path,
                    LastModifiedDate = file.LastModified.Value
                });
            }

            // Generate the list of the folder to be deleted from remote
            var remoteFolderItems = remoteItems.Where(item => item.Type == ItemType.Folder).ToArray();
            var localFolderItems = localItems.Where(item => item.Type == ItemType.Folder).ToArray();
            var foldersToDelete = remoteFolderItems.Except(localFolderItems, _equalityComparer).Cast<MegaNzItem>().ToArray();

            // Delete the remote folder if needed
            foreach(var folder in foldersToDelete)
            {
                commands.Add(new DeleteFolderCommand {
                    PathToDelete = folder.Path
                });
            }

            // Generate the list of folder to create
            var foldersToCreate = localFolderItems.Cast<IItem>().Except(remoteFolderItems, _equalityComparer).ToArray();
            foreach (var folder in foldersToCreate)
            {
                commands.Add(new CreateFolderCommand
                {
                    Name = folder.Name,
                    ParentPath = GetParentFolder(folder)
                });
            }
            // Generate the list of file to upload
            var filesToUpload = localFileItems.Cast<IItem>().Except(remoteFileItems, _equalityComparer).ToArray();
            foreach (var file in filesToUpload)
            {
                commands.Add(new UploadFileCommand {
                    SourcePath = LocalBasePath.TrimEnd('\\') + file.Path,
                    DestinationPath = GetParentFolder(file),
                    LastModifiedDate = file.LastModified.Value,
                    Size = file.Size
                });
            }

            return commands;
        }

        /// <summary>
        /// Return the parent folder for the item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        internal string GetParentFolder(IItem item)
        {
            var path = item.Path.TrimEnd('\\');

            if( path.Length == 0 )
            {
                // The item refers to the root folder
                return string.Empty;
            }

            var lastIndexSlash = path.LastIndexOf('\\');
            var result = path.Substring(0, lastIndexSlash);

            return result + "\\";
        }
    }
}
