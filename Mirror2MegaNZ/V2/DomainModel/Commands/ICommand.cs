using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirror2MegaNZ.V2.DomainModel.Commands
{
    internal interface ICommand
    {
        void Execute();
    }
}
