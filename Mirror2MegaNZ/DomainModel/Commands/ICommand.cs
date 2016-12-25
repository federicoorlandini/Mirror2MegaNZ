using CG.Web.MegaApiClient;
using Mirror2MegaNZ.Logic;
using System;

namespace Mirror2MegaNZ.DomainModel.Commands
{
    internal interface ICommand
    {
        void Execute(IMegaApiClient megaApiclient, 
            IMegaNzItemCollection megaNzItemCollection,
            IFileManager fileManager, 
            IProgress<double> progressNotifier);
    }
}
