using CG.Web.MegaApiClient;
using System;

namespace Mirror2MegaNZ.UnitTests.V2
{
    /// <summary>
    /// Implementation of the INode interface for testing purpose
    /// </summary>
    internal class MegaNzNodeMock : INode
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
            throw new NotImplementedException();
        }
    }
}
