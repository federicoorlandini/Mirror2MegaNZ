using CG.Web.MegaApiClient;
using Mirror2MegaNZ.DomainModel;
using System;

namespace Mirror2MegaNZ.Logic
{
    public class NodeConverter
    {
        public INode ToINode(MegaNZTreeNode treeNode)
        {
            throw new NotImplementedException();
        }

        public MegaNZTreeNode ToTreeNode(INode node)
        {
            return new MegaNZTreeNode
            {
                ObjectValue = node
            };
        }
    }
}
