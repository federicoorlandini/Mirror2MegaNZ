using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirror2MegaNZ.Logic
{
    /// <summary>
    /// A wrapper for the Console class, to have testable code
    /// </summary>
    public interface IConsoleWrapper
    {
        int CursorLeft { get; }
        int CursorTop { get; }
        void SetCursorPosition(int left, int top);
        void Write(string text);
    }
}
