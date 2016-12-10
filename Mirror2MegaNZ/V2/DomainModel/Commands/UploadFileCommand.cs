using CG.Web.MegaApiClient;
using Mirror2MegaNZ.Logic;
using Mirror2MegaNZ.V2.Logic;
using System;

namespace Mirror2MegaNZ.V2.DomainModel.Commands
{
    internal class UploadFileCommand  : ICommand
    {
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }

        /// <summary>
        /// Gets or sets the last modified date of the file to upload.
        /// We need this information because the remote filename will containt
        /// the last modified date of the original file
        /// </summary>
        /// <value>
        /// The last modified date.
        /// </value>
        public DateTime LastModifiedDate { get; set; }

        public void Execute(IMegaApiClient megaApiClient, 
            IMegaNzItemCollection megaNzItemCollection, 
            IFileManager fileManager,
            IProgress<double> progressNotifier)
        {
            using (var filestream = fileManager.GetStreamToReadFile(SourcePath))
            {
                var parentMegaNzNode = megaNzItemCollection.GetByPath(DestinationPath);
                megaApiClient.UploadAsync(filestream, SourcePath, parentMegaNzNode, progressNotifier).Wait();
            }
        }
    }
}
