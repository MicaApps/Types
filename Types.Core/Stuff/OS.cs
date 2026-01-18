using System.Runtime.InteropServices;

namespace Types.Core
{
    public static class OS
    {
        public static readonly int MajorVersion = System.Environment.OSVersion.Version.Major;
        public static readonly int MinorVersion = System.Environment.OSVersion.Version.Minor;
        
        public static readonly bool Vista = MajorVersion > 5;
        public static readonly bool Seven = Vista && MinorVersion > 0;
        
        [DllImport("shell32.dll")]
        static extern void SHChangeNotify (uint wEventId, uint uFlags, int dwItem1, int dwItem2);
        public static void FlushIcons () { SHChangeNotify(0x08000000, 0x0000, 0, 0); }
    }
}
