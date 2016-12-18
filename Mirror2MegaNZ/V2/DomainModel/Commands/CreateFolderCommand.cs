using CG.Web.MegaApiClient;
using Mirror2MegaNZ.Logic;
using Mirror2MegaNZ.V2.Logic;
using System;

namespace Mirror2MegaNZ.V2.DomainModel.Commands
{
    internal class CreateFolderCommand  : ICommand
    {
        public string Name { get; set; }
        public string ParentPath { get; set; }

        public void Execute(IMegaApiClient megaApiClient, 
            IMegaNzItemCollection megaNzItemCollection,
            IFileManager fileManager,
            IProgress<double> progressNotifier)
        {
            // Find the MegaNZ node related to the parent folder
            var parent = megaNzItemCollection.GetByPath(ParentPath);

            // Create the new folder
            var newNode = megaApiClient.CreateFolder(Name, parent);

            // Update the item collection
            megaNzItemCollection.Add(newNode);
        }

        public override string ToString()
        {
            return $"Create Folder Command - Name: {Name} - ParentPath: {ParentPath}";
        }
    }
}
