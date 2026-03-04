using client;
using Client;
using System;
using System.Windows.Forms;

namespace RemoteClient
{
    public partial class frmKetNoi : Form
    {
        private readonly ClientSocket client;

        public frmKetNoi(int userID, string role)
        {
            InitializeComponent();
            client = new ClientSocket();

            btnConnect.Click += BtnConnect_Click;
            btnDisconnect.Click += BtnDisconnect_Click;

            client.OnLog += Client_OnLog;
            client.OnConnected += Client_OnConnected;
            client.OnDisconnected += Client_OnDisconnected;
        }

        private void Client_OnDisconnected()
        {
            AddLog("Disconnected from server.");
        }

        private void Client_OnConnected()
        {
            // Cần Invoke vì sự kiện OnConnected thường được gọi từ luồng nền (background thread)
            // và chúng ta đang thao tác với UI (Form).
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(Client_OnConnected));
                return;
            }

            AddLog("Connected to server successfully.");

            try
            {
                // 1. Tạo form chức năng chính
                // Giả định frmClient nhận ClientSocket đã kết nối làm tham số đầu tiên
                // và các tham số khác (userID, role) được giữ nguyên.
                // Bạn cần thay thế userID và role bằng giá trị thực tế nếu cần.
                int userID = 0; // Thay bằng giá trị thực
                string role = "Client"; // Thay bằng giá trị thực

                // Tạo form điều khiển mới
                frmClient controlForm = new frmClient(client, userID, role);

                // 2. Ẩn form kết nối hiện tại và hiển thị form chức năng
                this.Hide();
                controlForm.Show();

                // **Quan trọng:** Gán sự kiện FormClosed để đóng toàn bộ ứng dụng khi form chức năng bị đóng.
                controlForm.FormClosed += (s, args) => this.Close();
            }
            catch (Exception ex)
            {
                AddLog($"Lỗi khi mở form điều khiển: {ex.Message}");
            }
        }

        private void Client_OnLog(string message)
        {
            AddLog(message);
        }

        private async void BtnConnect_Click(object sender, EventArgs e)
        {
            string host = txtHost.Text.Trim();
            if (!int.TryParse(txtPort.Text.Trim(), out int port))
            {
                MessageBox.Show("Port không hợp lệ!");
                return;
            }
            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name)) name = "Client";

            bool connected = await client.ConnectAsync(host, port, name);
            if (connected)
            {
                AddLog("Kết nối thành công!");
            }
            else
            {
                AddLog("Kết nối thất bại!");
            }
        }

        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            client.Disconnect();
        }

        private void AddLog(string msg)
        {
            if (listLog.InvokeRequired)
            {
                listLog.Invoke(new Action(() => listLog.Items.Add(msg)));
            }
            else
            {
                listLog.Items.Add(msg);
            }
        }
    }
}
