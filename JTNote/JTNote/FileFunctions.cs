using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTNote
{
    public static class FileFunctions
    {
        public static string GetContentOfFile(string path)
        {
            // Recreate with all lines to preserve newlines in incoming file
            string[] allLines = File.ReadAllLines(path);
            string output = string.Join(Environment.NewLine, allLines);

            return output;
        }
    }
}
