using CG.Web.MegaApiClient;
using Mirror2MegaNZ.Logic;
using Mirror2MegaNZ.V2.Logic;
using System;
using System.Collections.Generic;

namespace Mirror2MegaNZ.V2.DomainModel.Commands
{
    internal class DeleteFolderCommand : ICommand
    {
        public string PathToDelete { get; set; }

        public void Execute(IMegaApiClient megaApiClient, 
            IMegaNzItemCollection megaNzItemCollection,
            IFileManager fileManager,
            IProgress<double> progressNotifier)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"Delte File Command - PathToDelete: {PathToDelete}";
        }
    }
}
