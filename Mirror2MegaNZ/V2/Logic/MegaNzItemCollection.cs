using Mirror2MegaNZ.V2.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CG.Web.MegaApiClient;
using Mirror2MegaNZ.V2.DomainModel;

namespace Mirror2MegaNZ.V2.Logic
{
    internal class MegaNzItemCollection : IMegaNzItemCollection
    {
        private readonly Dictionary<string, INode> _nodeByPath;

        public MegaNzItemCollection(IEnumerable<MegaNzItem> collection)
        {
            _nodeByPath = collection.ToDictionary(item => item.Path, item => item.MegaNzNode);
        }

        public INode GetByPath(string path)
        {
            return _nodeByPath[path];
        }
    }
}
