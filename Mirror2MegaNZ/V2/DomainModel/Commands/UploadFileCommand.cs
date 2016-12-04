using System;

namespace Mirror2MegaNZ.V2.DomainModel.Commands
{
    internal class UploadFileCommand  : ICommand
    {
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
