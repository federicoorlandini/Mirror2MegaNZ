using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirror2MegaNZ.V2.DomainModel
{
    /// <summary>
    /// This is the generic interface for all the items we mirror.
    /// It can be applied to a local item (generated from a local file in the file system) or
    /// it can be applied to a node that comes from MegaNZ
    /// </summary>
    internal interface IItem
    {
        string Name { get; }
        ItemType Type { get; }
        string Path { get; }
        long Size { get; }
        DateTime? LastModified { get; }
    }
}
