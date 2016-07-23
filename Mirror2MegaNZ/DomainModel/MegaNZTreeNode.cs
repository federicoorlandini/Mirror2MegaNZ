using CG.Web.MegaApiClient;
using Mirror2MegaNZ.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mirror2MegaNZ.DomainModel
{
    public class MegaNZTreeNode
    {
        public MegaNZTreeNode()
        {
            ChildNodes = new List<MegaNZTreeNode>();
        }

        public INode ObjectValue { get; set; }

        public MegaNZTreeNode Parent { get; set; }
        public List<MegaNZTreeNode> ChildNodes { get; set; }

        public void AddChild(MegaNZTreeNode childNode)
        {
            childNode.Parent = this;
            ChildNodes.Add(childNode);
        }

        public void RemoveChild(MegaNZTreeNode childNodeToRemove)
        {
            var nodeToRemove = ChildNodes.SingleOrDefault(node => node.ObjectValue.Id == childNodeToRemove.ObjectValue.Id);
            if( nodeToRemove == null )
            {
                return;
            }

            ChildNodes.Remove(nodeToRemove);
            nodeToRemove.Parent = null;
        }
        
        public string NameWithoutLastModification
        {
            get
            {
                if (ObjectValue.Type == NodeType.Directory)
                {
                    return ObjectValue.Name;
                }
                else
                {
                    string filename;
                    DateTime datetime;
                    NameHandler.ExtractFilenameAndDateTimeFromRemoteFilename(ObjectValue.Name, out filename, out datetime);

                    return filename;
                }
            }
        }

        public DateTime LastModification
        {
            get
            {
                string extractedName;
                DateTime extractedDatetime;
                NameHandler.ExtractFilenameAndDateTimeFromRemoteFilename(ObjectValue.Name, out extractedName, out extractedDatetime);

                return extractedDatetime;
            }
        }
    }
}
