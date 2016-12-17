using CG.Web.MegaApiClient;
using Mirror2MegaNZ.Logic;
using Mirror2MegaNZ.V2.Logic;
using System;
using System.Collections.Generic;

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
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"Create Folder Command - Name: {Name} - ParentPath: {ParentPath}";
        }
    }
}
