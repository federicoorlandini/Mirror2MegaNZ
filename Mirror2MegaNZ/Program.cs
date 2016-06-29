using CG.Web.MegaApiClient;
using Mirror2MegaNZ.DomainModel;
using System;
using System.IO;
using System.Linq;
using Mirror2MegaNZ.Configuration;
using NLog;
using Mirror2MegaNZ.Logic;

namespace Mirror2MegaNZ
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            // Read the configuration
            var configuration = new ConfigurationReader();

            foreach(var account in configuration.Accounts)
            {
                logger.Trace("Processing the account {0}", account.Name);
                ProcessAccount(account, logger);
            }

            Console.WriteLine("Press any key to exit...");    
            Console.ReadLine();
        }

        private static void ProcessAccount(Account account, ILogger logger)
        {
            logger.Trace("Reading nodes from the folder {0}", account.LocalRoot);
            var localRoot = GenerateNodesFromFileSystem(account.LocalRoot);

            logger.Trace("Reading nodes from MegaNZ");
            MegaApiClient client = new MegaApiClient();
            client.Login(account.Username, account.Password);
            var remoteNodeList = client.GetNodes().ToList();

            logger.Trace("Building remote node tree");
            var treeBuilder = new TreeBuilder();
            var remoteRoot = treeBuilder.Build(remoteNodeList);

            logger.Trace("Starting synchroniziong...");
            var fileManager = new FileManager();
            var syncronizer = new Synchronizer(client, fileManager);
            syncronizer.SyncronizeFolder(localRoot, remoteRoot, logger);
        }

        private static LocalNode GenerateNodesFromFileSystem(string localRootFolder)
        {
            if( !Directory.Exists(localRootFolder) )
            {
                var message = string.Format("The local root folder {0} deosn't exist", localRootFolder);
                throw new InvalidOperationException(message);
            }
            
            DirectoryInfo localRootFolderInfo = new DirectoryInfo(localRootFolder);

            var root = new LocalNode
            {
                FullPath = localRootFolder,
                LastModificationDate = localRootFolderInfo.LastWriteTimeUtc,
                ParentNode = null,
                Name = localRootFolderInfo.Name,
                Type = NodeType.Directory
            };

            FileInfo[] files = localRootFolderInfo.GetFiles();
            DirectoryInfo[] directories = localRootFolderInfo.GetDirectories();

            // For each file, we add a new LocalNodes to the current node
            foreach(var file in files)
            {
                root.ChildNodes.Add(new LocalNode {
                    FullPath = file.FullName,
                    Name = Path.GetFileName(file.FullName),
                    LastModificationDate = file.LastWriteTimeUtc,
                    ParentNode = root,
                    Size = file.Length,
                    Type = NodeType.File
                });
            }

            // for each directory, we call the same same method recursively
            foreach(var directory in directories)
            {
                var childFolder = GenerateNodesFromFileSystem(directory.FullName);
                root.ChildNodes.Add(childFolder);
            }

            return root;
        }
    }
}
