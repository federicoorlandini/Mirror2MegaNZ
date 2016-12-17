using CG.Web.MegaApiClient;
using Mirror2MegaNZ.V2.DomainModel;

namespace Mirror2MegaNZ.V2.Logic
{
    internal interface IMegaNzItemCollection
    {
        INode GetByPath(string path);
        MegaNzItem Add(INode node);
        void RemoveItemByExactPath(string path);
    }
}
