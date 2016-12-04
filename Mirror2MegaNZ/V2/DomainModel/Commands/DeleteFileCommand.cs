using System;

namespace Mirror2MegaNZ.V2.DomainModel.Commands
{
    internal class DeleteFileCommand : ICommand
    {
        public string PathToDelete { get; set; }

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
