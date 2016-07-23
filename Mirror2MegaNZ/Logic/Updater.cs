using CG.Web.MegaApiClient;
using Mirror2MegaNZ.DomainModel;
using NLog;
using Polly;
using System;
using System.Linq;

namespace Mirror2MegaNZ.Logic
{
    /// <summary>
    /// This class compare the local tree with the remote tree and 
    /// update the remote tree to reflect the local tree
    /// </summary>
    public class Updater
    {
        private readonly IMegaApiClient _client;
        private readonly IFileManager _fileManager;
        private readonly IConsoleWrapper _consoleWrapper;

        public Updater(IMegaApiClient client, IFileManager fileManager, IConsoleWrapper consoleWrapper)
        {
            _client = client;
            _fileManager = fileManager;
            _consoleWrapper = consoleWrapper;
        }

        public void Update(LocalNode localRoot, MegaNZTreeNode remoteRoot, ILogger logger)
        {
            // Retrieve the files that are in the local root but no in the remote root
            var localFiles = localRoot.ChildNodes.Where(node => node.Type == NodeType.File).ToArray();
            var remoteFiles = remoteRoot.ChildNodes.Where(node => node.ObjectValue.Type == NodeType.File).ToArray();

            // Upload files that are not in the remote root
            var fileToUpload = localFiles.Where(localFile => !IsFileInRemote(localFile, remoteFiles)).ToArray();

            foreach(var file in fileToUpload)
            {
                Policy.Handle<Exception>()
                    .Retry(5, (ex, counter) => {
                        logger.Trace("An exception occurred: " + ex.GetType());
                        logger.Trace("Retry #" + counter);
                    })
                    .Execute(() =>
                    {
                        UploadFile(remoteRoot, logger, file);
                    });
            }

            // Update the folder that are in local and in remote
            var localFolders = localRoot.ChildNodes.Where(node => node.Type == NodeType.Directory).ToArray();
            var remoteFolders = remoteRoot.ChildNodes.Where(node => node.ObjectValue.Type == NodeType.Directory).ToArray();
            var foldersToUpdate = localFolders.Where(localFolder => IsFolderInRemote(localFolder, remoteFolders)).ToArray();
            foreach (var folderToUpdate in foldersToUpdate)
            {
                var remoteFolder = remoteFolders.Single(folder => NodeComparer.AreTheSameFolder(folder, folderToUpdate));
                Update(folderToUpdate, remoteFolder, logger);
            }

            // Create the folder that are in local but not in remote
            var foldersToCreate = localFolders.Where(localFolder => !IsFolderInRemote(localFolder, remoteFolders)).ToArray();

            var nodeConverter = new NodeConverter();
            foreach(var folder in foldersToCreate)
            {
                Policy.Handle<Exception>()
                    .Retry(10, (ex, counter) =>
                    {
                        logger.Trace("An exception occurred: " + ex.GetType());
                        logger.Trace("Retry #" + counter);
                    })
                    .Execute(() => {
                        CreateFolderInRemoteAndMoveToIt(remoteRoot, logger, nodeConverter, folder);
                    });
            }
        }

        /// <summary>
        /// Create a new folder in remote and move to update it 
        /// </summary>
        /// <param name="remoteRoot"></param>
        /// <param name="logger"></param>
        /// <param name="nodeConverter"></param>
        /// <param name="folder"></param>
        private void CreateFolderInRemoteAndMoveToIt(MegaNZTreeNode remoteRoot, ILogger logger, NodeConverter nodeConverter, LocalNode folder)
        {
            logger.Trace("Creating folder {0}", folder.Name);
            var remoteFolderName = NameHandler.BuildRemoteFolderName(folder.Name);
            var newNode = _client.CreateFolder(remoteFolderName, remoteRoot.ObjectValue);
            var newTreeNode = nodeConverter.ToTreeNode(newNode);
            remoteRoot.AddChild(newTreeNode);

            logger.Trace("Moving to folder {0}", folder.Name);
            Update(folder, newTreeNode, logger);
        }

        /// <summary>
        /// Upload a file in remote
        /// </summary>
        /// <param name="remoteRoot"></param>
        /// <param name="logger"></param>
        /// <param name="file"></param>
        private void UploadFile(MegaNZTreeNode remoteRoot, ILogger logger, LocalNode file)
        {
            logger.Trace("Uploading file {0} - size {1} in {2}", file.Name, FileSizeFormatter.Format(file.Size), remoteRoot.ObjectValue.Name);
            using (var fileStream = _fileManager.GetStreamToReadFile(file.FullPath))
            {
                var remoteFileName = NameHandler.BuildRemoteFileName(file.Name, file.LastModificationDate);
                var notifier = new ProgressNotifier(_consoleWrapper);
                _client.UploadAsync(fileStream, remoteFileName, remoteRoot.ObjectValue, notifier).Wait();
            }
            Console.WriteLine();
        }

        private bool IsFileInRemote(LocalNode localFile, MegaNZTreeNode[] remoteFiles)
        {
            return remoteFiles.Any(remoteFile => NodeComparer.AreTheSameFile(remoteFile, localFile));
        }

        private bool IsFolderInRemote(LocalNode localFolder, MegaNZTreeNode[] remoteFolders)
        {
            return remoteFolders.Any(remoteFolder => NodeComparer.AreTheSameFolder(remoteFolder, localFolder));
        }
    }
}
