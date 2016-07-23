using Mirror2MegaNZ.DomainModel;
using System;

namespace Mirror2MegaNZ.Logic
{
    internal static class NodeComparer
    {
        public static bool AreTheSameFile(MegaNZTreeNode file, LocalNode localFile)
        {
            if( file.ObjectValue.Type != CG.Web.MegaApiClient.NodeType.File ||
                localFile.Type != CG.Web.MegaApiClient.NodeType.File)
            {
                throw new InvalidOperationException("This method must be used to compare files");
            }

            // For the file, we compare name, size, type, and last modification datetime (without millisecond)
            return file.NameWithoutLastModification.Equals(localFile.Name, StringComparison.InvariantCultureIgnoreCase) &&
                   file.ObjectValue.Size == localFile.Size &&
                   file.ObjectValue.Type == localFile.Type &&
                   file.LastModification.Year == localFile.LastModificationDate.Year &&
                   file.LastModification.Month == localFile.LastModificationDate.Month &&
                   file.LastModification.Day == localFile.LastModificationDate.Day &&
                   file.LastModification.Hour == localFile.LastModificationDate.Hour &&
                   file.LastModification.Minute == localFile.LastModificationDate.Minute &&
                   file.LastModification.Second == localFile.LastModificationDate.Second;
        }

        public static bool AreTheSameFolder(MegaNZTreeNode folder, LocalNode localFolder)
        {
            if (folder.ObjectValue.Type != CG.Web.MegaApiClient.NodeType.Directory ||
                localFolder.Type != CG.Web.MegaApiClient.NodeType.Directory)
            {
                throw new InvalidOperationException("This method must be used to compare folders");
            }

            // For the folder, we compare only the name
            return folder.ObjectValue.Name.Equals(localFolder.Name, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
