using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.Projects
{
    public class DirectoryStructureComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            var xParts = x.Replace("\\", "/").Split('/');
            var yParts = y.Replace("\\", "/").Split('/');

            for (int i = 0; i <= Math.Min(xParts.Length - 1, yParts.Length - 1); i++)
            {
                if (string.Compare(xParts[i], yParts[i], StringComparison.CurrentCultureIgnoreCase) != 0)
                {
                    return xParts[i].CompareTo(yParts[i]);
                }
            }

            //If we get here, then we have two directories like:
            //Test/Ing
            //Test/Ing/Dir

            //We want to make sure Test/Ing comes before Test/Ing/Dir
            return xParts.Length.CompareTo(yParts.Length);
        }
    }
}
