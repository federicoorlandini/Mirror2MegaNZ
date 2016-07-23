using System;

namespace Mirror2MegaNZ.Logic
{
    /// <summary>
    /// This is a simple wrapper for the Console static class
    /// to be ale to unit test the code
    /// </summary>
    public class ConsoleWrapper : IConsoleWrapper
    {
        public int CursorLeft
        {
            get { return Console.CursorLeft; }
        }

        public int CursorTop
        {
            get { return Console.CursorTop; }
        }

        public void SetCursorPosition(int left, int top)
        {
            Console.SetCursorPosition(left, top);
        }

        public void Write(string text)
        {
            Console.Write(text);
        }
    }
}
