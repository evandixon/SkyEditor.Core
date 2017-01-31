using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.ConsoleCommands
{
    /// <summary>
    /// An IConsoleProvider that does nothing, existing to allow running ConsoleCommands outside of an environment with a console.
    /// </summary>
    public class DummyConsoleProvider : IConsoleProvider
    {

        public ConsoleColor BackgroundColor
        {
            get { return ConsoleColor.Black; }
            set { }
        }

        public ConsoleColor ForegroundColor
        {
            get { return ConsoleColor.White; }
            set { }
        }

        public void Write(char[] value)
        {
        }

        public void Write(string value)
        {
        }

        public void Write(object value)
        {
        }

        public void Write(bool value)
        {
        }

        public void Write(string format, params object[] arg)
        {
        }

        public void Write(char[] value, int index, int count)
        {
        }

        public void WriteLine()
        {
        }

        public void WriteLine(char[] value)
        {
        }

        public void WriteLine(object value)
        {
        }

        public void WriteLine(string value)
        {
        }

        public void WriteLine(string format, params object[] arg)
        {
        }

        public void WriteLine(char[] value, int index, int count)
        {
        }

        public int Read()
        {
            return 0;
        }

        public string ReadLine()
        {
            return string.Empty;
        }
    }
}
