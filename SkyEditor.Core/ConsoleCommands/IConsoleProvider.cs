using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.ConsoleCommands
{
    public interface IConsoleProvider
    {
        ConsoleColor BackgroundColor { get; set; }
        ConsoleColor ForegroundColor { get; set; }
        int Read();
        string ReadLine();
        void Write(bool value);
        void Write(char[] value);
        void Write(char[] value, int index, int count);
        void Write(object value);
        void Write(string value);
        void Write(string format, params object[] arg);
        void WriteLine();
        void WriteLine(string value);
        void WriteLine(char[] value);
        void WriteLine(char[] value, int index, int count);
        void WriteLine(object value);
        void WriteLine(string format, params object[] arg);
    }
}
