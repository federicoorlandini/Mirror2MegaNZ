using CG.Web.MegaApiClient;
using Mirror2MegaNZ.V2.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirror2MegaNZ.V2.Logic
{
    /// <summary>
    /// This class generate a MegaNzItem collection starting from
    /// a collection of MegaNZ nodes
    /// </summary>
    internal class MegaNzItemListGenerator
    {
        public IEnumerable<MegaNzItem> Generate(IEnumerable<INode> nodes)
        {
            var nodeCollection = nodes.ToDictionary(node => node.Id);

            return nodes
                .Where(node => node.Type != NodeType.Inbox && node.Type != NodeType.Trash)
                .Select(node => new MegaNzItem(node, nodeCollection))
                .ToArray();
        }
    }
}
