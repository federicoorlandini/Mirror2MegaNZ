using CG.Web.MegaApiClient;
using System;

namespace Mirror2MegaNZ.DomainModel
{
    public class MegaNZNode : INode
    {
        public string Id { get; set; }
        public DateTime LastModificationDate { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public string ParentId { get; set; }
        public long Size { get; set; }
        public NodeType Type { get; set; }

        public bool Equals(INode other)
        {
            return Id == other.Id;
        }
    }
}
