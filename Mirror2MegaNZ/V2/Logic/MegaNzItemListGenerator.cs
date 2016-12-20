using CG.Web.MegaApiClient;
using Mirror2MegaNZ.V2.DomainModel;
using System.Collections.Generic;
using System.Linq;

namespace Mirror2MegaNZ.V2.Logic
{
    /// <summary>
    /// This class generate a MegaNzItem collection starting from
    /// a collection of MegaNZ nodes
    /// </summary>
    internal class MegaNzItemListGenerator
    {
        public IEnumerable<MegaNzItem> Generate(List<INode> nodes)
        {
            var nodeByIdIndex = nodes.ToDictionary(node => node.Id, node => node);

            // Build a lookup based on the ParentID node
            var nodesByParentIdIndex = (from node in nodes
                                          group node by node.ParentId into g
                                          select new { ParentID = g.Key, ChildrenNodes = g })
                                         .ToDictionary(item => item.ParentID, item => item.ChildrenNodes.ToArray());

            // We need to filter the nodes to remove the TRASH folder, the INBOX folder and
            // all the files that are their children
            nodeByIdIndex = FilterRemoteNodeList(nodeByIdIndex, nodesByParentIdIndex);

            return nodeByIdIndex
                .Select(item => new MegaNzItem(item.Value, nodeByIdIndex))
                .ToArray();
        }

        private Dictionary<string, INode> FilterRemoteNodeList(Dictionary<string, INode> nodeByIdIndex, 
            Dictionary<string, INode[]> nodesByParentIdIndex)
        {
            var inboxFolderItems = nodeByIdIndex.Where(item => item.Value.Type == NodeType.Inbox);
            var trashFolderItems = nodeByIdIndex.Where(item => item.Value.Type == NodeType.Trash);

            if(!inboxFolderItems.Any() && !trashFolderItems.Any())
            {
                return nodeByIdIndex; // Nothing to remove
            }

            if( inboxFolderItems.Any() )
            {
                var inboxFolderItem = inboxFolderItems.Single();    // We should have only one Inbox folder
                RemoveNodeHierarchy(inboxFolderItem.Value, nodeByIdIndex, nodesByParentIdIndex);
            }

            if( trashFolderItems.Any() )
            {
                var trashFolderItem = trashFolderItems.Single();    // We should have only one trash folder
                RemoveNodeHierarchy(trashFolderItem.Value, nodeByIdIndex, nodesByParentIdIndex);
            }

            return nodeByIdIndex;
        }

        private void RemoveNodeHierarchy(INode nodeToRemove, 
            Dictionary<string, INode> nodeByIdIndex, 
            Dictionary<string, INode[]> nodesByParentIdIndex)
        {
            // If the Node ID is not in the nodesByParentIdIndex then the node hasn't any children
            if( nodesByParentIdIndex.ContainsKey(nodeToRemove.Id))
            {
                var childrens = nodesByParentIdIndex[nodeToRemove.Id];
                foreach (var child in childrens)
                {
                    RemoveNodeHierarchy(child, nodeByIdIndex, nodesByParentIdIndex);
                }
            }

            nodeByIdIndex.Remove(nodeToRemove.Id);
        }
    }
}
