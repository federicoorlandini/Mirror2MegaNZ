using CG.Web.MegaApiClient;
using Mirror2MegaNZ.DomainModel;

namespace Mirror2MegaNZ.Logic
{
    internal interface IMegaNzItemCollection
    {
        INode GetByPath(string path);
        MegaNzItem Add(INode node);
        void RemoveItemByExactPath(string path);
    }
}
