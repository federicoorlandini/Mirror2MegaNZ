using CG.Web.MegaApiClient;
using Mirror2MegaNZ.DomainModel;
using Mirror2MegaNZ.V2.DomainModel;
using Mirror2MegaNZ.V2.DomainModel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirror2MegaNZ.V2.Logic
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

            // Generate the list of the file and folder to be deleted from remote
            var commands = new List<ICommand>();
            var itemsToDelete = remoteItems.Except(localItems, _equalityComparer).Cast<MegaNzItem>().ToArray();
            foreach (var item in itemsToDelete)
            {
                ICommand command;
                switch (item.Type)
                {
                    case ItemType.File:
                        command = new DeleteFileCommand
                        {
                            PathToDelete = item.Path
                        };
                        break;

                    case ItemType.Folder:
                        command = new DeleteFolderCommand
                        {
                            PathToDelete = item.Path
                        };
                        break;

                    default:
                        throw new InvalidOperationException("The type " + item.Type.ToString() + " is invalid");
                }
                commands.Add(command);
            }

            // Generate the list of file and folder to upload
            var itemsToUpload = localItems.Cast<IItem>().Except(remoteItems, _equalityComparer).ToArray();
            foreach (var item in itemsToUpload)
            {
                ICommand command;

                switch(item.Type)
                {
                    case ItemType.File:
                        command = new UploadFileCommand
                        {
                            SourcePath = LocalBasePath.TrimEnd('\\') + item.Path,
                            DestinationPath = GetParentFolder(item)
                        };
                        break;

                    case ItemType.Folder:
                        command = new CreateFolderCommand
                        {
                            Name = item.Name,
                            ParentPath = GetParentFolder(item)
                        };
                        break;

                    default:
                        throw new InvalidOperationException("The type " + item.Type.ToString() + " is invalid");
                }
                

                commands.Add(command);
            }

            return commands;
        }

        /// <summary>
        /// Return the parent folder for the item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private string GetParentFolder(IItem item)
        {
            var path = item.Path.TrimEnd(new[] { '\\' });

            if( path.Length == 0 )
            {
                // The item refers to the root folder
                return string.Empty;
            }

            var lastIndexSlash = path.LastIndexOf('\\');
            return path.Substring(0, lastIndexSlash + 1);
        }
    }
}
