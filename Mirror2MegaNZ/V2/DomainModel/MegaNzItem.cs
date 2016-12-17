using CG.Web.MegaApiClient;
using Mirror2MegaNZ.Logic;
using System;
using System.Collections.Generic;

namespace Mirror2MegaNZ.V2.DomainModel
{
    internal class MegaNzItem : IItem
    {
        public string Name { get; private set; }
        public ItemType Type { get; private set; }
        public string Path { get; private set; }
        public long Size { get; private set; }
        public DateTime? LastModified { get; private set;}

        public INode MegaNzNode { get; private set; }

        public MegaNzItem(INode megaNzNode, string name, ItemType type, string path, long size, DateTime? lastModified = null)
        {
            if( type == ItemType.Folder && lastModified != null)
            {
                throw new InvalidOperationException("A MegaNzItem that represent a folder cannot have a lastModified date");
            }

            MegaNzNode = megaNzNode;
            Name = name;
            Type = type;
            Path = path;
            Size = size;
            LastModified = lastModified;
        }

        public MegaNzItem(INode megaNzNode, Dictionary<string, INode> megaNzNodeDictionary)
        {
            MegaNzNode = megaNzNode;

            switch (megaNzNode.Type)
            {
                case NodeType.Root:
                    Name = @"\";
                    Type = ItemType.Folder;
                    Path = @"\";
                    Size = 0;
                    break;

                case NodeType.Directory:
                    Name = megaNzNode.Name;
                    Type = ItemType.Folder;
                    Path = BuildPath(megaNzNode, megaNzNodeDictionary);
                    Size = 0;
                    break;

                case NodeType.File:
                    Type = ItemType.File;
                    Path = BuildPath(megaNzNode, megaNzNodeDictionary);
                    Size = megaNzNode.Size;

                    // Extract the filename, removing the last modification date
                    string filename;
                    DateTime datetime;
                    NameHandler.ExtractFilenameAndDateTimeFromRemoteFilename(megaNzNode.Name, out filename, out datetime);
                    Name = filename;
                    LastModified = datetime;
                    break;

                default:
                    throw new InvalidOperationException("Not able to create an item for INode that are not Directory or File");
            }
        }

        private string BuildPath(INode megaNzNode, Dictionary<string, INode> megaNzNodeDictionary)
        {
            if (megaNzNode.Type == NodeType.Root)
            {
                return @"\";
            }

            if (megaNzNode.Type != NodeType.File)
            {
                var parentNode = megaNzNodeDictionary[megaNzNode.ParentId];
                var path = BuildPath(parentNode, megaNzNodeDictionary) + megaNzNode.Name
;
                // we need to add a slash at the end to correctly build the path
                return path + @"\";
            }
            else
            {
                // Extract the filename, removing the last modification date (only for file)
                string filename;
                DateTime datetime;
                NameHandler.ExtractFilenameAndDateTimeFromRemoteFilename(megaNzNode.Name, out filename, out datetime);

                var parentNode = megaNzNodeDictionary[megaNzNode.ParentId];
                var path = BuildPath(parentNode, megaNzNodeDictionary) + filename;

                return path;
            }
        }
    }
}
