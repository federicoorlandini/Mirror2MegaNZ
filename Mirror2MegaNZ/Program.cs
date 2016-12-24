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
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            // Read the configuration
            var configuration = new ConfigurationReader();

            foreach(var account in configuration.Accounts.Where(account => account.Synchronize))
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
            var itemListFromFileSystem = GenerateListFromLocalFolder(account.LocalRoot, account.LocalRoot);

            MegaApiClient client = new MegaApiClient();
            client.Login(account.Username, account.Password);

            var itemListFromMegaNz = GenerateListFromMegaNz(client).ToList();

            var commandGenerator = new CommandGenerator(account.LocalRoot);
            var commandList = commandGenerator.GenerateCommandList(itemListFromFileSystem, itemListFromMegaNz);

            if( !commandList.Any() )
            {
                Console.WriteLine("Nothing to do here. Exiting.....");
                return;
            }

            // Showing the commands and asking if continue
            ShowCommandList(commandList);
            if( commandList.OfType<DeleteFileCommand>().Any())
            {
                Console.WriteLine("There are some files to delete. Continue? (y/n)");
                var continueAnswer = Console.ReadLine();
                if( continueAnswer.ToLower() != "y" )
                {
                    Console.WriteLine("Exiting...");
                }
            }

            // Executing the commands in the list
            var fileManager = new FileManager();
            var progressNotifier = new ProgressNotifier(new ConsoleWrapper());
            var executor = new CommandExecutor(client);

            var megaNzItemCollection = new MegaNzItemCollection(itemListFromMegaNz);
            executor.Execute(commandList, megaNzItemCollection, fileManager, progressNotifier);
        }

        private static void ShowCommandList(List<ICommand> commandList)
        {
            Console.WriteLine();
            Console.WriteLine("##### Command list #####");
            Console.WriteLine("Command list:");
            foreach(var command in commandList)
            {
                Console.WriteLine(command.ToString());
                Console.WriteLine();
            }
            Console.WriteLine("Command list resume:");
            Console.WriteLine("Upload file command: " + commandList.OfType<UploadFileCommand>().Count());
            Console.WriteLine("Create folder command: " + commandList.OfType<CreateFolderCommand>().Count());
            Console.WriteLine("Delete folder command: " + commandList.OfType<DeleteFolderCommand>().Count());
            Console.WriteLine("Delete file command: " + commandList.OfType<DeleteFileCommand>().Count());
            Console.WriteLine("##### End of Command list #####");
            Console.WriteLine("Press any key to exit or 'X' to stop the execution");

            var key = Console.ReadKey();
            if( key.Key == ConsoleKey.X )
            {
                Environment.Exit(0);
            }
        }

        private static IEnumerable<MegaNzItem> GenerateListFromMegaNz(IMegaApiClient megaApiClient)
        {
            logger.Trace("Reading nodes from MegaNZ");
            var remoteNodeList = megaApiClient
                .GetNodes()
                .ToList();

            var generator = new MegaNzItemListGenerator();
            return generator.Generate(remoteNodeList);
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
