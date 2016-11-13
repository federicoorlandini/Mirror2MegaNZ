using CG.Web.MegaApiClient;
using Mirror2MegaNZ.DomainModel;
using NLog;
using System.Linq;

namespace Mirror2MegaNZ.Logic
{
    /// <summary>
    /// This is the delegate for the event triggered by the cleaner when it delete a file.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="RemoteDeletingEventArgs"/> instance containing the event data.</param>
    public delegate void OnRemoteDeletingHandler(object sender, RemoteDeletingEventArgs e);

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

        /// <summary>
        /// Occurs just before the Cleaner delete a remote file.
        /// </summary>
        public event OnRemoteDeletingHandler OnRemoteDeleting;

        /// <summary>
        /// Cleans up the remote folder based on the content of the local folder
        /// </summary>
        /// <param name="localRoot">The local root.</param>
        /// <param name="remoteRoot">The remote root.</param>
        /// <param name="logger">The logger.</param>
        public void CleanUp(LocalNode localRoot, MegaNZTreeNode remoteRoot, ILogger logger)
        {
            logger.Trace("Clening up remote node for " + localRoot.Name);

            // Retrieve the files that are in the remote root but no in the local root
            var localFiles = localRoot
                .ChildNodes
                .Where(node => node.Type == NodeType.File)
                .ToDictionary(item => item.HashCode);
            var remoteFiles = remoteRoot
                .ChildNodes
                .Where(node => node.ObjectValue.Type == NodeType.File)
                .ToDictionary(item => item.HashCode);

            var filesToRemove = remoteFiles
                .Where(file => !localFiles.ContainsKey(file.Key))
                .Select(item => item.Value)
                .ToArray();

            foreach (var file in filesToRemove)
            {
                logger.Trace("Deleting file {0} from remote", file.ObjectValue.Name);

                // Trigger the event
                var eventArgs = new RemoteDeletingEventArgs
                {
                    Filename = file.ObjectValue.Name
                };
                OnRemoteDeleting?.Invoke(this, eventArgs);
                if( eventArgs.Cancel )
                {
                    logger.Trace("Skipping the deletion for the file {0} from remote", eventArgs.Filename);
                    continue;
                }

                if( eventArgs.Cancel )
                {
                    logger.Trace("Delete canceled");
                }
                else
                {
                    _client.Delete(file.ObjectValue);
                    remoteRoot.RemoveChild(file);
                }
            }

            // Clean up the remote folder that are not in the local file system
            var localFolders = localRoot.ChildNodes.Where(node => node.Type == NodeType.Directory).ToArray();
            var remoteFolders = remoteRoot.ChildNodes.Where(node => node.ObjectValue.Type == NodeType.Directory).ToArray();

            var foldersToRemove = remoteFolders.Where(folder => !IsFolderInLocal(folder, localFolders)).ToArray();

            foreach(var folder in foldersToRemove)
            {
                // Trigger the event
                var eventArgs = new RemoteDeletingEventArgs
                {
                    Filename = folder.NameWithoutLastModification
                };
                OnRemoteDeleting?.Invoke(this, eventArgs);
                if( eventArgs.Cancel )
                {
                    logger.Trace("Skipping the deletion for the folder {0} from remote", folder.ObjectValue.Name);
                    continue;
                }
                
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

        private bool IsFolderInLocal(MegaNZTreeNode folder, LocalNode[] localFolders)
        {
            return localFolders.Any(localFolder => NodeComparer.AreTheSameFolder(folder, localFolder));
        }
    }
}
