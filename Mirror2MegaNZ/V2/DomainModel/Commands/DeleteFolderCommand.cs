using System;

namespace Mirror2MegaNZ.V2.DomainModel.Commands
{
    internal class DeleteFolderCommand : ICommand
    {
        public string PathToDelete { get; set; }

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
