using System;

namespace Mirror2MegaNZ.Logic
{
    internal class ProgressNotifier : IProgress<double>
    {
        private readonly IConsoleWrapper _consoleWrapper;

        public ProgressNotifier(IConsoleWrapper consoleWrapper)
        {
            _consoleWrapper = consoleWrapper;
        }

        public void Report(double value)
        {
            _consoleWrapper.SetCursorPosition(_consoleWrapper.CursorLeft, _consoleWrapper.CursorTop);
            _consoleWrapper.Write(string.Format("\r{0}%", value.ToString("F1")));
        }
    }
}
