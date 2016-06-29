using CG.Web.MegaApiClient;
using System;
using System.Collections.Generic;

namespace Mirror2MegaNZ.DomainModel
{
    public class LocalNode
    {
        public NodeType Type { get; set; }
        public string Name { get; set; }
        public string FullPath { get; set; }
        public long Size { get; set; }
        public DateTime LastModificationDate { get; set; }

        public LocalNode ParentNode { get; set; }
        public List<LocalNode> ChildNodes { get; set; }

        public LocalNode()
        {
            ChildNodes = new List<LocalNode>();
        }

        public bool IsRoot
        {
            get { return ParentNode == null; }
        }

        public void AddChild(LocalNode childNode)
        {
            childNode.ParentNode = this;
            ChildNodes.Add(childNode);
        }
    }
}
