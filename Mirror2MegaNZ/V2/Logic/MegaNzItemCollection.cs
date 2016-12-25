using System.Collections.Generic;
using System.Linq;
using CG.Web.MegaApiClient;
using Mirror2MegaNZ.V2.DomainModel;

namespace Mirror2MegaNZ.V2.Logic
{
    internal class MegaNzItemCollection : IMegaNzItemCollection
    {
        private readonly Dictionary<string, INode> _nodeByPath;
        private readonly Dictionary<string, INode> _nodeByMegaNzNodeId;
        private readonly List<MegaNzItem> _collection;

        public MegaNzItemCollection(IEnumerable<MegaNzItem> collection)
        {
            _collection = collection.ToList();
            _nodeByPath = collection.ToDictionary(item => item.Path, item => item.MegaNzNode);
            _nodeByMegaNzNodeId = collection.ToDictionary(item => item.MegaNzNode.Id, item => item.MegaNzNode);
        }
                
        public INode GetByPath(string path)
        {
            return _nodeByPath[path];
        }

        public MegaNzItem Add(INode node)
        {
            var newItem = new MegaNzItem(node, _nodeByMegaNzNodeId);
            _collection.Add(newItem);
            _nodeByPath[newItem.Path] = node;
            _nodeByMegaNzNodeId[newItem.MegaNzNode.Id] = node;
            return newItem;
        }

        public void RemoveItemByExactPath(string path)
        {
            var itemToRemove = _collection.Single(item => item.Path == path);
            _collection.Remove(itemToRemove);

            if( _nodeByPath.ContainsKey(path))
            {
                var node = GetByPath(path);
                _nodeByPath.Remove(path);
                _nodeByMegaNzNodeId.Remove(node.Id);
            }
        }

        /// <summary>
        /// Gets the list of all the items in the collection
        /// </summary>
        /// <returns></returns>
        public List<MegaNzItem> GetList()
        {
            return _collection.ToList();
        }
    }
}
