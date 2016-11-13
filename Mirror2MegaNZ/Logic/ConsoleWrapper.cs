using System;

namespace Mirror2MegaNZ.Logic
{
    /// <summary>
    /// This is a simple wrapper for the Console static class
    /// to be ale to unit test the code
    /// </summary>
    public class ConsoleWrapper : IConsoleWrapper
    {
        /// <summary>
        /// Gets the left position of the cursor in the console.
        /// </summary>
        /// <value>
        /// The cursor left.
        /// </value>
        public int CursorLeft
        {
            get { return Console.CursorLeft; }
        }

        /// <summary>
        /// Gets the top position of the cursor in the console.
        /// </summary>
        /// <value>
        /// The cursor top.
        /// </value>
        public int CursorTop
        {
            get { return Console.CursorTop; }
        }

        /// <summary>
        /// Sets the cursor position.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="top">The top.</param>
        public void SetCursorPosition(int left, int top)
        {
            Console.SetCursorPosition(left, top);
        }

        /// <summary>
        /// Writes the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Write(string text)
        {
            Console.Write(text);
        }
    }
}
