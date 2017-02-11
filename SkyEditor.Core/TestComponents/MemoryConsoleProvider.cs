using System;
using System.Collections.Generic;
using System.Text;
using SkyEditor.Core.ConsoleCommands;

namespace SkyEditor.Core.TestComponents
{
    public class MemoryConsoleProvider : IConsoleProvider
    {

        public MemoryConsoleProvider()
        {
            BackgroundColor = ConsoleColor.Black;
            ForegroundColor = ConsoleColor.White;
            StdIn = new StringBuilder();
            StdOut = new StringBuilder();
        }

        public ConsoleColor BackgroundColor { get; set; }

        public ConsoleColor ForegroundColor { get; set; }

        public StringBuilder StdIn { get; set; }

        private object _stdInLock = new object();
        public StringBuilder StdOut { get; set; }

        private object _stdOutLock = new object();
        public void Write(char[] value)
        {
            lock (_stdOutLock)
            {
                foreach (var c in value)
                {
                    StdOut.Append(c);
                }
            }
        }

        public void Write(string value)
        {
            lock (_stdOutLock)
            {
                StdOut.Append(value);
            }
        }

        public void Write(object value)
        {
            lock (_stdOutLock)
            {
                StdOut.Append(value.ToString());
            }
        }

        public void Write(bool value)
        {
            lock (_stdOutLock)
            {
                StdOut.Append(value.ToString());
            }
        }

        public void Write(string format, params object[] arg)
        {
            lock (_stdOutLock)
            {
                StdOut.AppendFormat(format, arg);
            }
        }

        public void Write(char[] value, int index, int count)
        {
            lock (_stdOutLock)
            {
                StdOut.Append(value, index, count);
            }
        }

        public void WriteLine()
        {
            lock (_stdOutLock)
            {
                StdOut.Append(Environment.NewLine);
            }
        }

        public void WriteLine(char[] value)
        {
            lock (_stdOutLock)
            {
                foreach (var c in value)
                {
                    StdOut.Append(c);
                }
                StdOut.Append(Environment.NewLine);
            }
        }

        public void WriteLine(object value)
        {
            lock (_stdOutLock)
            {
                StdOut.Append(value.ToString());
                StdOut.Append(Environment.NewLine);
            }
        }

        public void WriteLine(string value)
        {
            lock (_stdOutLock)
            {
                StdOut.Append(value);
                StdOut.Append(Environment.NewLine);
            }
        }

        public void WriteLine(string format, params object[] arg)
        {
            lock (_stdOutLock)
            {
                StdOut.AppendFormat(format, arg);
                StdOut.Append(Environment.NewLine);
            }
        }

        public void WriteLine(char[] value, int index, int count)
        {
            lock (_stdOutLock)
            {
                StdOut.Append(value, index, count);
                StdOut.Append(Environment.NewLine);
            }
        }

        public int Read()
        {
            char? c;
            lock (_stdInLock)
            {
                if (StdIn.Length > 0)
                {
                    c = StdIn[0];
                    StdIn.Remove(0, 1);
                }
                else
                {
                    c = null;
                }
            }
            return c.Value;
        }

        public string ReadLine()
        {
            if (StdIn.Length == 0)
            {
                // End of input
                return null;
            }

            StringBuilder line = new StringBuilder();
            lock (_stdInLock)
            {
                while (StdIn.Length > 0 && (line.Length == 0 || line[line.Length - 1] != '\n'))
                {
                    line.Append(StdIn[0]);
                    StdIn.Remove(0, 1);
                }
            }
            return line.ToString().Trim();
        }

        public string GetStdOut()
        {
            lock (_stdOutLock)
            {
                return StdOut.ToString();
            }
        }

    }
}
