using CG.Web.MegaApiClient;
using Mirror2MegaNZ.DomainModel;
using NLog;
using System.Linq;

namespace Mirror2MegaNZ.Logic
{
    /// <summary>
    /// This class clean up the remote MegaNZ account, removing the
    /// remote items that are not in the local repository any more
    /// </summary>
    public class Cleaner
    {
        private readonly IMegaApiClient _client;

        public Cleaner(IMegaApiClient client)
        {
            _client = client;
        }

        public void CleanUp(LocalNode localRoot, MegaNZTreeNode remoteRoot, ILogger logger)
        {
            // Retrieve the files that are in the remote root but no in the local root
            var localFiles = localRoot.ChildNodes.Where(node => node.Type == NodeType.File).ToArray();
            var remoteFiles = remoteRoot.ChildNodes.Where(node => node.ObjectValue.Type == NodeType.File).ToArray();

            var filesToRemove = remoteFiles.Where(file => !IsFileInLocal(file, localFiles)).ToArray();

            foreach (var file in filesToRemove)
            {
                logger.Trace("Deleting file {0} from remote", file.ObjectValue.Name);
                _client.Delete(file.ObjectValue);
                remoteRoot.RemoveChild(file);
            }

            // Clean up the remote folder that are not in the local file system
            var localFolders = localRoot.ChildNodes.Where(node => node.Type == NodeType.Directory).ToArray();
            var remoteFolders = remoteRoot.ChildNodes.Where(node => node.ObjectValue.Type == NodeType.Directory).ToArray();

            var foldersToRemove = remoteFolders.Where(folder => !IsFolderInLocal(folder, localFolders)).ToArray();

            foreach(var folder in foldersToRemove)
            {
                logger.Trace("Deleting folder {0} from remote", folder.ObjectValue.Name);
                _client.Delete(folder.ObjectValue);
                remoteRoot.RemoveChild(folder);
            }

            // We must clean up the remote folder that exists in the local file system
            var foldersToCleanUp = remoteFolders.Except(foldersToRemove).ToArray();
            foreach(var folderToCleanUp in foldersToCleanUp)
            {
                var localFolder = localFolders.Single(folder => NodeComparer.AreTheSameFolder(folderToCleanUp, folder));
                CleanUp(localFolder, folderToCleanUp, logger);
            }
        }

        private bool IsFileInLocal(MegaNZTreeNode file, LocalNode[] localFiles)
        {
            return localFiles.Any(localFile => NodeComparer.AreTheSameFile(file, localFile));
        }

        private bool IsFolderInLocal(MegaNZTreeNode folder, LocalNode[] localFolders)
        {
            return localFolders.Any(localFolder => NodeComparer.AreTheSameFolder(folder, localFolder));
        }
    }
}
