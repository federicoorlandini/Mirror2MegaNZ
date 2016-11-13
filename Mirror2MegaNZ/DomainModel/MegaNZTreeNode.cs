using CG.Web.MegaApiClient;
using Mirror2MegaNZ.Logic;
using System;
using System.Collections.Generic;
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

        public void AddChild(MegaNZTreeNode childNode)
        {
            childNode.Parent = this;
            ChildNodes.Add(childNode);
        }

        public void RemoveChild(MegaNZTreeNode childNodeToRemove)
        {
            var nodeToRemove = ChildNodes.SingleOrDefault(node => node.ObjectValue.Id == childNodeToRemove.ObjectValue.Id);
            if (nodeToRemove == null)
            {
                return;
            }

            ChildNodes.Remove(nodeToRemove);
            nodeToRemove.Parent = null;
        }

        public int HashCode
        {
            get { return GetHashCode(); }
        }

        public override int GetHashCode()
        {
            // For the folders, we use the remote name hash
            if( ObjectValue.Type == NodeType.Directory )
            {
                return ObjectValue.Name.GetHashCode();
            }

            unchecked // Overflow is fine, just wrap
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ NameWithoutLastModification.ToLower().GetHashCode();
                hash = (hash * 16777619) ^ ObjectValue.Size.GetHashCode();
                hash = (hash * 16777619) ^ ObjectValue.Type.GetHashCode();
                hash = (hash * 16777619) ^ LastModification.Year.GetHashCode();
                hash = (hash * 16777619) ^ LastModification.Month.GetHashCode();
                hash = (hash * 16777619) ^ LastModification.Day.GetHashCode();
                hash = (hash * 16777619) ^ LastModification.Hour.GetHashCode();
                hash = (hash * 16777619) ^ LastModification.Minute.GetHashCode();
                hash = (hash * 16777619) ^ LastModification.Second.GetHashCode();
                return hash;
            }
        }
    }
}
