using CG.Web.MegaApiClient;
using System;
using System.Collections.Generic;

namespace Mirror2MegaNZ.DomainModel
{
    /// <summary>
    /// This class is the abstraction for a local file that we want to mirror remotely
    /// </summary>
    public class LocalNode
    {
        public LocalNode()
        {
            ChildNodes = new List<LocalNode>();
        }

        /// <summary>
        /// Gets or sets the type of the local node.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public NodeType Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the local file (including the extension).
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the full path that point to the local file.
        /// </summary>
        /// <value>
        /// The full path.
        /// </value>
        public string FullPath { get; set; }

        /// <summary>
        /// Gets or sets the size in byte of the file.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the last modification date for the local file
        /// </summary>
        /// <value>
        /// The last modification date.
        /// </value>
        public DateTime LastModificationDate { get; set; }

        /// <summary>
        /// Gets or sets the parent node.
        /// </summary>
        /// <value>
        /// The parent node.
        /// </value>
        public LocalNode ParentNode { get; set; }

        /// <summary>
        /// Gets or sets the child nodes.
        /// </summary>
        /// <value>
        /// The child nodes.
        /// </value>
        public List<LocalNode> ChildNodes { get; set; }

        public int HashCode
        {
            get { return GetHashCode(); }
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

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ Name.ToLower().GetHashCode();
                hash = (hash * 16777619) ^ Size.GetHashCode();
                hash = (hash * 16777619) ^ Type.GetHashCode();
                hash = (hash * 16777619) ^ LastModificationDate.Year.GetHashCode();
                hash = (hash * 16777619) ^ LastModificationDate.Month.GetHashCode();
                hash = (hash * 16777619) ^ LastModificationDate.Day.GetHashCode();
                hash = (hash * 16777619) ^ LastModificationDate.Hour.GetHashCode();
                hash = (hash * 16777619) ^ LastModificationDate.Minute.GetHashCode();
                hash = (hash * 16777619) ^ LastModificationDate.Second.GetHashCode();
                return hash;
            }
        }
    }
}
