using CG.Web.MegaApiClient;
using Mirror2MegaNZ.V2.DomainModel;
using Mirror2MegaNZ.V2.DomainModel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirror2MegaNZ.V2.Logic
{
    /// <summary>
    /// This is the class that execute the command specified in the ICommand items
    /// </summary>
    internal class CommandExecutor
    {
        private readonly IMegaApiClient _megaApiClient;
        
        public CommandExecutor(IMegaApiClient megaApiClient, List<MegaNzItem> remoteItems)
        {
            _megaApiClient = megaApiClient;
            MegaNzItems = remoteItems;
        }
          
        public List<MegaNzItem> MegaNzItems { get; private set; }
    
        public void Execute(IEnumerable<ICommand> commands)
        {

        }
    }
}
