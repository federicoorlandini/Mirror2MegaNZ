using CG.Web.MegaApiClient;
using Mirror2MegaNZ.Logic;
using Mirror2MegaNZ.V2.DomainModel.Commands;
using System;
using System.Collections.Generic;

namespace Mirror2MegaNZ.V2.Logic
{
    /// <summary>
    /// This is the class that execute the command specified in the ICommand items
    /// </summary>
    internal class CommandExecutor
    {
        private readonly IMegaApiClient _megaApiClient;
        
        public CommandExecutor(IMegaApiClient megaApiClient)
        {
            _megaApiClient = megaApiClient;
        }
          
        public void Execute(IEnumerable<ICommand> commands, 
            IMegaNzItemCollection megaNzItemCollection,
            IFileManager fileManager,
            IProgress<double> progressNotifier)
        {
            foreach(var command in commands)
            {
                Console.WriteLine();
                Console.WriteLine(command.ToString());
                command.Execute(_megaApiClient, megaNzItemCollection, fileManager, progressNotifier);
            }
        }
    }
}
