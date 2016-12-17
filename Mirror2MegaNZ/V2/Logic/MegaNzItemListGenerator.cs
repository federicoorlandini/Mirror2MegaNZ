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
        public IEnumerable<MegaNzItem> Generate(IEnumerable<INode> nodes)
        {
            var nodeCollection = nodes.ToDictionary(node => node.Id, node => node);

            return nodes
                .Select(node => new MegaNzItem(node, nodeCollection))
                .ToArray();
        }
    }
}
