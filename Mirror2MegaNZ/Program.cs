using CG.Web.MegaApiClient;
using Mirror2MegaNZ.DomainModel;
using System;
using System.IO;
using System.Linq;
using Mirror2MegaNZ.Configuration;
using NLog;
using Mirror2MegaNZ.Logic;
using System.Collections.Generic;
using Mirror2MegaNZ.V2.DomainModel;
using SystemInterface.IO;
using SystemWrapper.IO;
using Mirror2MegaNZ.V2.Logic;
using Mirror2MegaNZ.V2.DomainModel.Commands;

namespace Mirror2MegaNZ
{
    class Program
    {
        // MIGLIORE SOLUZIONE: invece di creare un albero di nodi e confrontare l'albero del file system
        // con questo albero dei nodi possiamo:
        // 1. dal file system creare una lista di nomi di file, comprensivi del path da cui però abbiamo
        //    rimosso la parte di path relativa alla cartella root the stiamo sincronizzando
        // 2. dalla lista degli INode remoti, creare una lista di di nomi di file, comprensivi del path
        // 3. Utilizzando queste due liste, possiamo ottenere in modo semplice la lista dei file da aggiungere e 
        //    rimuovere da remoto (usando ad esempio la Intersec() e la Except() di LINQ)
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
            //----------- V1 ------------
            //logger.Trace("Reading nodes from the folder {0}", account.LocalRoot);
            //var localRoot = GenerateNodesFromFileSystem(account.LocalRoot);

            //logger.Trace("Reading nodes from MegaNZ");
            //MegaApiClient client = new MegaApiClient();
            //client.Login(account.Username, account.Password);
            //var remoteNodeList = client.GetNodes().ToList();

            //logger.Trace("Building remote node tree");
            //var treeBuilder = new TreeBuilder();
            //var remoteRoot = treeBuilder.Build(remoteNodeList);

            //logger.Trace("Starting synchroniziong...");
            //var fileManager = new FileManager();
            //var consoleWrapper = new ConsoleWrapper();
            //var syncronizer = new Synchronizer(client, fileManager, consoleWrapper);
            //syncronizer.RemoteFileDeletingHandler += Syncronizer_RemoteFileDeletingHandler;
            //syncronizer.SyncronizeFolder(localRoot, remoteRoot, logger);
            //----------- End V1 ------------
            logger.Trace("Reading nodes from the folder {0}", account.LocalRoot);
            var itemListFromFileSystem = GenerateListFromLocalFolder(account.LocalRoot, account.LocalRoot);

            MegaApiClient client = new MegaApiClient();
            client.Login(account.Username, account.Password);

            var itemListFromMegaNz = GenerateListFromMegaNz(client).ToList();

            var commandGenerator = new CommandGenerator(account.LocalRoot);
            var commandList = commandGenerator.GenerateCommandList(itemListFromFileSystem, itemListFromMegaNz);

            ShowCommandList(commandList);

            var fileManager = new FileManager();
            var progressNotifier = new ProgressNotifier(new ConsoleWrapper());
            var executor = new CommandExecutor(client);

            var megaNzItemCollection = new MegaNzItemCollection(itemListFromMegaNz);
            executor.Execute(commandList, megaNzItemCollection, fileManager, progressNotifier);
        }

        private static void ShowCommandList(List<ICommand> commandList)
        {
            Console.WriteLine("Command list:");
            foreach(var command in commandList)
            {
                Console.WriteLine(command.ToString());
            }
        }

        private static IEnumerable<MegaNzItem> GenerateListFromMegaNz(IMegaApiClient megaApiClient)
        {
            logger.Trace("Reading nodes from MegaNZ");
            var remoteNodeList = megaApiClient
                .GetNodes()
                .ToList();

            // We need to filter the nodes to remove the TRASH folder, the INBOX folder and
            // all the files that are their children
            remoteNodeList = FilterRemoteNodeList(remoteNodeList);

            var generator = new MegaNzItemListGenerator();
            return generator.Generate(remoteNodeList);
        }

        private static List<INode> FilterRemoteNodeList(List<INode> remoteNodeList)
        {
            var inboxFolder = remoteNodeList.Single(node => node.Type == NodeType.Inbox);
            var trashFolder = remoteNodeList.Single(node => node.Type == NodeType.Trash);

            return remoteNodeList.Where(node => node.Id != inboxFolder.Id &&
                    node.Id != trashFolder.Id &&
                    node.ParentId != inboxFolder.Id &&
                    node.ParentId != trashFolder.Id)
                .ToList();
        }

        private static void Syncronizer_RemoteFileDeletingHandler(object sender, RemoteDeletingEventArgs e)
        {
            Console.WriteLine($"Do you want to delete the remote file {e.Filename}? (Y/N)");
            var result = Console.ReadKey();
            e.Cancel = result.Key != ConsoleKey.Y;
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

        private static List<FileItem> GenerateListFromLocalFolder(string localRootFolder, string basePath)
        {
            if (!Directory.Exists(localRootFolder))
            {
                var message = string.Format("The local root folder {0} deosn't exist", localRootFolder);
                throw new InvalidOperationException(message);
            }

            IDirectoryInfo localRootFolderInfo = new DirectoryInfoWrap(new DirectoryInfo(localRootFolder));
            var listGenerator = new LocalFileItemListGenerator();
            var list = listGenerator.Generate(localRootFolderInfo, basePath);
            return list;
        }
    }
}
