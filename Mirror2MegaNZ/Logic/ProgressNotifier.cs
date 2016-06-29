using System;

namespace Mirror2MegaNZ.Logic
{
    internal class ProgressNotifier : IProgress<double>
    {
        private readonly int _cursorPositionLeft;
        private readonly int _cursorPositionTop;

        public ProgressNotifier(int cursorPositionLeft, int cursorPositionTop)
        {
            _cursorPositionLeft = cursorPositionLeft;
            _cursorPositionTop = cursorPositionTop;
        }

        public void Report(double value)
        {
            Console.SetCursorPosition(_cursorPositionLeft, _cursorPositionTop);
            Console.Write(string.Format("\r{0}%", value.ToString("F1")));
        }
    }
}
