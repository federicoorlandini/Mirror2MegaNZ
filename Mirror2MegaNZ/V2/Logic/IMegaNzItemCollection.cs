using CG.Web.MegaApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirror2MegaNZ.V2.Logic
{
    internal interface IMegaNzItemCollection
    {
        INode GetByPath(string path);
    }
}
