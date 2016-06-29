using CG.Web.MegaApiClient;
using Mirror2MegaNZ.DomainModel;
using System.Collections.Generic;
using System.Linq;

namespace Mirror2MegaNZ.Logic
{
    /// <summary>
    /// This class contains only a method to build a Tree of INode
    /// starting from a plain list
    /// </summary>
    public class TreeBuilder
    {
        public MegaNZTreeNode Build(IEnumerable<INode> nodeCollection)
        {
            var nodeConverter = new NodeConverter();

            var lookup = nodeCollection
                .Select(item => nodeConverter.ToTreeNode(item))
                .ToDictionary(item => item.ObjectValue.Id);

            foreach(var node in nodeCollection)
            {
                var treeNode = lookup[node.Id];

                // Find the children
                var childrenNodes = nodeCollection.Where(n => n.ParentId == node.Id).ToList();
                treeNode.ChildNodes = childrenNodes.Select(childNode => lookup[childNode.Id]).ToList();

                // Assign the parent to all the children
                foreach(var child in treeNode.ChildNodes)
                {
                    child.Parent = treeNode;
                }
            }

            return lookup.Single(n => n.Value.ObjectValue.Type == NodeType.Root).Value;
        }
    }
}
