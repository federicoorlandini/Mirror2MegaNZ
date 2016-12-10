using CG.Web.MegaApiClient;
using Mirror2MegaNZ.Logic;
using Mirror2MegaNZ.V2.Logic;
using System;

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
    }
}
