using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Threading.Tasks;

namespace Types.WinUI
{
    public static class IconExtractor
    {
        [DllImport("Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int ExtractIconEx(string sFile, int iIndex, out IntPtr piLargeVersion, out IntPtr piSmallVersion, int amountIcons);

        [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        private static extern int DestroyIcon(IntPtr hIcon);

        public static async Task<BitmapImage> GetIconFromResource(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            string filePath = path;
            int index = 0;

            if (path.Contains(","))
            {
                var parts = path.Split(',');
                filePath = parts[0];
                if (parts.Length > 1)
                {
                    int.TryParse(parts[1], out index);
                }
            }
            
            // Expand environment variables (e.g. %SystemRoot%)
            filePath = Environment.ExpandEnvironmentVariables(filePath);

            if (!File.Exists(filePath)) return null;

            IntPtr large, small;
            // Extract 1 icon
            ExtractIconEx(filePath, index, out large, out small, 1);

            try
            {
                // Prefer small icon for list view (16x16), fallback to large
                IntPtr hIcon = small != IntPtr.Zero ? small : large;
                
                if (hIcon == IntPtr.Zero) return null;

                using (var icon = System.Drawing.Icon.FromHandle(hIcon))
                using (var bmp = icon.ToBitmap())
                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Png);
                    ms.Position = 0;

                    var bitmapImage = new BitmapImage();
                    var ras = new InMemoryRandomAccessStream();
                    var writer = new DataWriter(ras.GetOutputStreamAt(0));
                    writer.WriteBytes(ms.ToArray());
                    await writer.StoreAsync();
                    
                    await bitmapImage.SetSourceAsync(ras);
                    return bitmapImage;
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                if (large != IntPtr.Zero) DestroyIcon(large);
                if (small != IntPtr.Zero) DestroyIcon(small);
            }
        }
    }
}
