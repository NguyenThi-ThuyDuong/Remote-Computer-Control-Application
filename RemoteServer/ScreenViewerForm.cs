using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System;
using System.Threading;

namespace Server
{
    // Form này dùng để hiển thị hình ảnh từ client
    public partial class ScreenViewerForm : Form
    {
        public ScreenViewerForm()
        {
            InitializeComponent();
            this.Text = "Remote Screen Viewer";
            // Cài đặt PictureBox để tự động điều chỉnh kích thước
            pictureBoxScreen.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBoxScreen.Dock = DockStyle.Fill;
        }

        /// <summary>
        /// Nhận chuỗi Base64 và hiển thị lên PictureBox.
        /// </summary>
        /// <param name="base64Image">Chuỗi Base64 của hình ảnh JPEG.</param>
        public void DisplayScreen(string base64Image)
        {
            if (this.IsDisposed) return;

            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64Image);

                using (var ms = new MemoryStream(imageBytes))
                {
                    // Tạo Image từ MemoryStream
                    var img = Image.FromStream(ms);

                    // Cần Invoke để cập nhật UI từ Thread nền của ServerSocket
                    this.Invoke((MethodInvoker)delegate
                    {
                        // Dispose ảnh cũ để giải phóng bộ nhớ
                        if (pictureBoxScreen.Image != null)
                        {
                            pictureBoxScreen.Image.Dispose();
                        }
                        pictureBoxScreen.Image = img;
                        this.Text = $"Remote Screen Viewer ({img.Width}x{img.Height})";
                    });
                }
            }
            catch (Exception)
            {
                // Xử lý lỗi Base64 hoặc chuyển đổi hình ảnh
            }
        }
    }
}