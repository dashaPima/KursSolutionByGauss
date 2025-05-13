using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KyrsovRPVS
{
    public class IniFile
    {
        private readonly string _path;
        public IniFile(string path) => _path = path;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        public string Read(string section, string key, string defaultValue = "")
        {
            var sb = new StringBuilder(1024);
            GetPrivateProfileString(section, key, defaultValue, sb, sb.Capacity, _path);
            return sb.ToString();
        }
        public void Write(string section, string key, string value)
            => WritePrivateProfileString(section, key, value, _path);
    }
}
