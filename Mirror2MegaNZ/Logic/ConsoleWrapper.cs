using System;
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
