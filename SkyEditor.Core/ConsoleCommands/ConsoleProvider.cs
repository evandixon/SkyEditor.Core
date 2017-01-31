using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.ConsoleCommands
{
    public class WindowsConsoleProvider : IConsoleProvider
    {

        public ConsoleColor BackgroundColor
        {
            get { return Console.BackgroundColor; }
            set { Console.BackgroundColor = value; }
        }

        public ConsoleColor ForegroundColor
        {
            get { return Console.ForegroundColor; }
            set { Console.ForegroundColor = value; }
        }

        public void Write(char[] value)
        {
            Console.Write(value);
        }

        public void Write(string value)
        {
            Console.Write(value);
        }

        public void Write(object value)
        {
            Console.Write(value);
        }

        public void Write(bool value)
        {
            Console.Write(value);
        }

        public void Write(string format, params object[] arg)
        {
            Console.Write(format, arg);
        }

        public void Write(char[] value, int index, int count)
        {
            Console.Write(value, index, count);
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }

        public void WriteLine(char[] value)
        {
            Console.WriteLine(value);
        }

        public void WriteLine(object value)
        {
            Console.WriteLine(value);
        }

        public void WriteLine(string value)
        {
            Console.WriteLine(value);
        }

        public void WriteLine(string format, params object[] arg)
        {
            Console.WriteLine(format, arg);
        }

        public void WriteLine(char[] value, int index, int count)
        {
            Console.WriteLine(value, index, count);
        }

        public int Read()
        {
            return Console.Read();
        }

        public string ReadLine()
        {
            return Console.ReadLine();
        }
    }
}
