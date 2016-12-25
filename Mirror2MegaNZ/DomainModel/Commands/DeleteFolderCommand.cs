using CG.Web.MegaApiClient;
using Mirror2MegaNZ.Logic;
using System;

namespace Mirror2MegaNZ.DomainModel.Commands
{
    internal class DeleteFolderCommand : ICommand
    {
        public string PathToDelete { get; set; }

        public void Execute(IMegaApiClient megaApiClient, 
            IMegaNzItemCollection megaNzItemCollection,
            IFileManager fileManager,
            IProgress<double> progressNotifier)
        {
            var nodeToDelete = megaNzItemCollection.GetByPath(PathToDelete);
            megaApiClient.Delete(nodeToDelete);
            megaNzItemCollection.RemoveItemByExactPath(PathToDelete);
        }

        public override string ToString()
        {
            return $"Delete File Command - PathToDelete: {PathToDelete}";
        }
    }
}
