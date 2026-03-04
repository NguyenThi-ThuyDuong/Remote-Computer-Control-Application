using server;
using server.Network;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq; 

namespace Server
{
    public partial class frmChat : Form
    {
        private readonly ServerSocket server;
        private readonly ClientInfo targetClient; 
        private bool isViewingScreen = false;
        private readonly string role;
        public frmChat(ServerSocket server, ClientInfo c, string userRole) // <<< Cập nhật: Nhận userRole
        {
            InitializeComponent();
            this.server = server ?? throw new ArgumentNullException(nameof(server));
            this.targetClient = c ?? throw new ArgumentNullException(nameof(c), "Target ClientInfo must not be null.");
            this.role = userRole; // <<< Gán Vai trò
            this.Text = $"Điều khiển: {c.Name}";

            this.Load += FrmChat_Load;

            // Đăng ký sự kiện từ ServerSocket (Chỉ lắng nghe client hiện tại)
            server.OnClientConnected += (client) => { if (client.Id == targetClient.Id) AddLog($"[+] Client kết nối: {client.Name}"); RefreshList(); };
            server.OnClientDisconnected += (client) => { if (client.Id == targetClient.Id) AddLog($"[-] Client ngắt kết nối: {client.Name}"); RefreshList(); };
            // SỬA: Lọc tin nhắn chỉ hiển thị thông báo/CHAT/RESP
            server.OnMessageReceived += (client, msg) =>
            {
                if (client.Id == targetClient.Id)
                {
                    // Lọc tin SCREEN ra khỏi log
                    if (!msg.StartsWith("SCREEN|"))
                        AddLog($"[{client.Name}]: {msg}");
                }
            };
            server.OnScreenReceived += Server_OnScreenReceived;

            // Gắn sự kiện cho các nút bấm
            if (btnGui != null) btnGui.Click += BtnGui_Click;
            if (btnScreen != null) btnScreen.Click += BtnScreen_Click;
            if (btnRunCMD != null) btnRunCMD.Click += BtnRunCMD_Click;
            this.btnRestart.Click += BtnRestart_Click;
            this.btnShutdown.Click += BtnShutdown_Click;
            this.btnSleep.Click += BtnSleep_Click;

            //  ÁP DỤNG PHÂN QUYỀN NÂNG CAO NGAY KHI KHỞI TẠO FORM
            ApplyChatPermissions();
        }

        // =============================================================
        // 1. CHUẨN HÓA: HÀM GỬI LỆNH DUY NHẤT (Đã đổi tên và thêm kiểm tra kết nối)
        // =============================================================

        /// <summary>
        /// Hàm chung gửi lệnh CMD tới Client cố định (targetClient).
        /// </summary>
        private async Task SendCmd(string cmd, string data = "")
        {
            // Kiểm tra kết nối trước khi gửi
            bool isConnected = server.GetClients().Any(c => c.Id == targetClient.Id);
            if (!isConnected)
            {
                MessageBox.Show($"Client {targetClient.Name} hiện không kết nối!", "Lỗi Gửi Lệnh");
                return;
            }

            // Đóng gói lệnh thành chuỗi định dạng mạng (Ví dụ: "CMD|SHUTDOWN" hoặc "CMD|EXEC|ipconfig")
            string packet = string.IsNullOrEmpty(data) ? $"CMD|{cmd}" : $"CMD|{cmd}|{data}";

            // GỬI GÓI TIN QUA KẾT NỐI SOCKET TỚI CLIENT CỐ ĐỊNH
            await server.SendToClientAsync(targetClient.Id, packet);

            AddLog($"[ME -> {targetClient.Name}] Lệnh: {cmd} {data}");
        }

        // =============================================================
        // 2. CÁC NÚT ĐIỀU KHIỂN (Đã sửa và dùng SendCmd mới)
        // =============================================================

        private async void BtnSleep_Click(object sender, EventArgs e)
        {
            await SendCmd("SLEEP");
        }

        private async void BtnShutdown_Click(object sender, EventArgs e)
        {
            await SendCmd("SHUTDOWN");
        }

        private async void BtnRestart_Click(object sender, EventArgs e)
        {
            await SendCmd("RESTART");
        }

        private void BtnScreen_Click(object sender, EventArgs e)
        {
            if (!isViewingScreen)
            {
                SendCmd("SCREEN_START"); // Bắt đầu (sử dụng SendCmd đã được sửa)
                btnScreen.Text = "Dừng Xem";
                isViewingScreen = true;
            }
            else
            {
                SendCmd("SCREEN_STOP"); // Dừng
                btnScreen.Text = "Xem Màn Hình";
                isViewingScreen = false;
                // Tùy chọn: Xóa ảnh cũ trên PictureBox nếu có
                // if (picScreen.Image != null) { picScreen.Image.Dispose(); picScreen.Image = null; }
            }
        }

        private void BtnRunCMD_Click(object sender, EventArgs e)
        {
            // GIẢ ĐỊNH: Bạn có TextBox tên là txtLenhCMD và txtChat
            string cmd = txtLenhCMD.Text.Trim();

            if (string.IsNullOrEmpty(cmd))
            {
                cmd = txtChat.Text.Trim();
            }

            if (string.IsNullOrEmpty(cmd))
            {
                MessageBox.Show("Vui lòng nhập lệnh CMD!", "Thiếu lệnh");
                return;
            }

            // Gửi lệnh EXEC (sử dụng SendCmd đã được sửa)
            SendCmd("EXEC", cmd);
            txtLenhCMD.Clear();
        }

        private async void BtnGui_Click(object sender, EventArgs e)
        {
            // KHÔNG CẦN cmbClients.SelectedItem nữa, chỉ cần targetClient
            if (!string.IsNullOrEmpty(txtChat.Text))
            {
                // Gửi trực tiếp tin nhắn CHAT tới targetClient
                await server.SendToClientAsync(targetClient.Id, "CHAT|" + txtChat.Text);
                AddLog($"[Me]: {txtChat.Text}");
                txtChat.Clear();
            }
        }

        // =============================================================
        // 3. LOGIC XỬ LÝ KHÁC (Cần giả định các control UI)
        // =============================================================

        private void FrmChat_Load(object sender, EventArgs e) { RefreshList(); }

        // --- Xử lý nhận ảnh màn hình ---
        private void Server_OnScreenReceived(ClientInfo c, string base64)
        {
            // CHỈ XỬ LÝ ẢNH CỦA targetClient
            if (c.Id != targetClient.Id) return;

            try
            {
                // Giả định picScreen tồn tại và được đặt tên đúng
                PictureBox pic = this.Controls["picScreen"] as PictureBox;
                if (pic == null) return;

                byte[] b = Convert.FromBase64String(base64);
                using (MemoryStream ms = new MemoryStream(b))
                {
                    Image img = Image.FromStream(ms);
                    if (pic.InvokeRequired)
                    {
                        pic.Invoke(new Action(() => {
                            if (pic.Image != null) pic.Image.Dispose();
                            pic.Image = new Bitmap(img);
                        }));
                    }
                    else
                    {
                        if (pic.Image != null) pic.Image.Dispose();
                        pic.Image = new Bitmap(img);
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog($"Lỗi nhận màn hình: {ex.Message}");
            }
        }

        // --- Helper UI ---
        private void AddLog(string s)
        {
            // Giả định lstChat tồn tại và được đặt tên đúng
            if (lstChat == null) return;

            if (lstChat.InvokeRequired) Invoke(new Action(() => AddLog(s)));
            else
            {
                lstChat.Items.Add($"[{DateTime.Now:HH:mm:ss}] {s}");
                lstChat.TopIndex = lstChat.Items.Count - 1;
            }
        }

        // HÀM BỔ SUNG: LOGIC PHÂN QUYỀN TRONG FORM CHAT
        // =============================================================

        private void ApplyChatPermissions()
        {
            // Kiểm tra vai trò Admin (không phân biệt hoa thường)
            bool isAdmin = (this.role != null && this.role.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase));

            AddLog($"[QUYỀN] Người dùng hiện tại: {this.role}.");

            // --- GIỚI HẠN CÁC TÍNH NĂNG NGUY HIỂM (Chỉ Admin) ---

            // 1. Xem Màn Hình (Remote Screen)
            if (btnScreen != null)
            {
                btnScreen.Tag = isAdmin ? "AdminOnly" : "UserNotAllowed";
                // Không bật Enable tại đây. Nó sẽ được bật trong RefreshList() nếu Client đang kết nối VÀ là Admin.
            }

            // 2. Chạy Lệnh CMD (Remote Execution)
            if (btnRunCMD != null)
            {
                btnRunCMD.Tag = isAdmin ? "AdminOnly" : "UserNotAllowed";
            }
            // Giả định txtLenhCMD là control nhập lệnh
            // TextBox cho lệnh CMD nên được vô hiệu hóa nếu không phải Admin
            TextBox txtCmd = this.Controls["txtLenhCMD"] as TextBox;
            if (txtCmd != null)
            {
                txtCmd.Enabled = isAdmin;
                if (!isAdmin) txtCmd.Text = "Bạn không có quyền chạy lệnh CMD.";
            }

            // 3. Các nút điều khiển hệ thống (Shutdown, Restart, Sleep)
            // Mặc dù đã được lọc ở frmMainServer, ta lọc lại ở đây cho chắc chắn.
            if (btnShutdown != null) btnShutdown.Tag = isAdmin ? "AdminOnly" : "UserNotAllowed";
            if (btnRestart != null) btnRestart.Tag = isAdmin ? "AdminOnly" : "UserNotAllowed";
            if (btnSleep != null) btnSleep.Tag = isAdmin ? "AdminOnly" : "UserNotAllowed";

            // Nút Chat (Thường cho phép tất cả mọi người)
            if (btnGui != null) btnGui.Tag = "Allowed";

            // Gọi RefreshList để áp dụng trạng thái kết nối + quyền
            RefreshList();
        }

        // =============================================================
        // CẬP NHẬT: REFRESH LIST (Áp dụng trạng thái kết nối VÀ Quyền)
        // =============================================================

        private void RefreshList()
        {
            // Giả định cmbClients tồn tại và được đặt tên đúng
            if (cmbClients == null) return;

            if (cmbClients.InvokeRequired) { Invoke(new Action(RefreshList)); return; }

            // Lấy trạng thái kết nối
            bool isConnected = server.GetClients().Any(c => c.Id == targetClient.Id);

            // Kiểm tra quyền Admin
            bool isAdmin = (this.role != null && this.role.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase));

            // Xóa và chỉ thêm targetClient vào (để hiển thị)
            cmbClients.Items.Clear();
            cmbClients.Items.Add(new ComboItem(targetClient));
            cmbClients.SelectedIndex = 0;

            // Kích hoạt/Vô hiệu hóa nút dựa trên trạng thái kết nối VÀ quyền

            // Chat
            if (btnGui != null) btnGui.Enabled = isConnected;

            // Chức năng Admin: Chỉ bật nếu kết nối VÀ là Admin
            bool adminAccess = isConnected && isAdmin;

            if (btnShutdown != null) btnShutdown.Enabled = adminAccess;
            if (btnRestart != null) btnRestart.Enabled = adminAccess;
            if (btnSleep != null) btnSleep.Enabled = adminAccess;

            // Xem màn hình
            if (btnScreen != null) btnScreen.Enabled = adminAccess;

            // Chạy CMD
            if (btnRunCMD != null) btnRunCMD.Enabled = adminAccess;

            // TextBox CMD (cần kiểm tra null)
            TextBox txtCmd = this.Controls["txtLenhCMD"] as TextBox;
            if (txtCmd != null)
            {
                txtCmd.Enabled = adminAccess;
            }
        }

        private class ComboItem
        {
            public ClientInfo Info;
            public ComboItem(ClientInfo i) { Info = i; }
            public override string ToString() => Info.Name;
        }
    }
}