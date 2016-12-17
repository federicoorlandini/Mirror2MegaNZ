using CG.Web.MegaApiClient;
using Mirror2MegaNZ.Logic;
using Mirror2MegaNZ.V2.Logic;
using System;

namespace Mirror2MegaNZ.V2.DomainModel.Commands
{
    internal interface ICommand
    {
        void Execute(IMegaApiClient megaApiclient, 
            IMegaNzItemCollection megaNzItemCollection,
            IFileManager fileManager, 
            IProgress<double> progressNotifier);
    }
}
