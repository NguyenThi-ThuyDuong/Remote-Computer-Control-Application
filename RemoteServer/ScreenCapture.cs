using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemoteServer
{
    public static class ScreenCapture
    {
        /// <summary>
        /// Chụp màn hình chính và chuyển thành chuỗi Base64 (JPEG)
        /// </summary>
        /// <returns>Chuỗi Base64 hoặc null nếu lỗi.</returns>
        public static string CaptureScreenToBase64()
        {
            try
            {
                var screen = Screen.PrimaryScreen.Bounds;
                using (Bitmap bmp = new Bitmap(screen.Width, screen.Height))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        // Chụp toàn bộ màn hình vào đối tượng Bitmap
                        g.CopyFromScreen(screen.Left, screen.Top, 0, 0, bmp.Size);
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Lưu Bitmap dưới dạng JPEG để giảm kích thước
                        bmp.Save(ms, ImageFormat.Jpeg);

                        // Chuyển byte array sang Base64 string
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
