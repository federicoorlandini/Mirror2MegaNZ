using System;

namespace Mirror2MegaNZ.V2.DomainModel.Commands
{
    internal class CreateFolderCommand  : ICommand
    {
        public string Name { get; set; }
        public string ParentPath { get; set; }

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
