using server;
using server.Database;
using server.Network;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Server
{
    public partial class frmMainServer : Form
    {
        // 🌟 Sửa: Dùng readonly
        private readonly ServerSocket server;
        private readonly DatabaseLogger dbLogger;
        private readonly int serverPort = 9000;
        private string role;

        // 🌟 Sửa: Loại bỏ tham số không dùng nếu chúng không được sử dụng bên trong constructor
        public frmMainServer(int userID) // Hoặc giữ lại (int userID, string role) nếu cần dùng ở nơi khác
        {
            InitializeComponent();

            dbLogger = new DatabaseLogger("Data Source=HP;Initial Catalog=RemoteControlDB;Integrated Security=True");

            server = new ServerSocket();
            server.OnLog += Server_OnLog;
            server.OnClientConnected += Server_OnClientConnected;
            server.OnClientDisconnected += Server_OnClientDisconnected;
            server.OnMessageReceived += Server_OnMessageReceived;

            try
            {
                server.Start(serverPort);
                AddLog($"[INFO] Server bắt đầu lắng nghe tại port {serverPort}");
            }
            catch (Exception ex)
            {
                AddLog($"[LỖI] Không thể khởi động Server: {ex.Message}");
            }

            // Disable nút lúc khởi tạo
            btnShutdown.Enabled = false;
            btnRestart.Enabled = false;
            btnSleep.Enabled = false;
            btnChat.Enabled = false;

            // Gán sự kiện
            btnShutdown.Click += BtnShutdown_Click;
            btnRestart.Click += BtnRestart_Click;
            btnSleep.Click += BtnSleep_Click;
            btnChat.Click += BtnChat_Click;

            // ListView event
            lstClients.SelectedIndexChanged += LstClients_SelectedIndexChanged;
        }
      
        public frmMainServer(int userID, string role) : this(userID)
        {
            this.role = role;
            ApplyPermissions();
        }

        #region Server events
        private void Server_OnLog(string msg)
        {
            AddLog(msg);
            dbLogger.Log("ServerLog", msg);
        }
        private void ApplyPermissions()
        {
            if (string.IsNullOrEmpty(this.role)) return;

            // Kiểm tra vai trò
            bool isAdmin = (this.role.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase));

            // Ghi log trạng thái đăng nhập
            AddLog($"[QUYỀN] Đăng nhập với vai trò: {this.role}.");

           

            // Chức năng điều khiển hệ thống: Chỉ Admin mới có Tag này
            if (btnShutdown != null) btnShutdown.Tag = isAdmin ? "AdminOnly" : "UserNotAllowed";
            if (btnRestart != null) btnRestart.Tag = isAdmin ? "AdminOnly" : "UserNotAllowed";
            if (btnSleep != null) btnSleep.Tag = isAdmin ? "AdminOnly" : "UserNotAllowed";

            // Chức năng Chat/Điều khiển chi tiết: Cho phép tất cả
            if (btnChat != null) btnChat.Tag = "Allowed";
        }
        private void Server_OnClientConnected(ClientInfo c)
        {
            AddLog($"[+] Client kết nối: {c.Name} [{c.Id}]");
            UpdateClientList();
        }

        private void Server_OnClientDisconnected(ClientInfo c)
        {
            AddLog($"[-] Client ngắt kết nối: {c.Name} [{c.Id}]");
            UpdateClientList();
        }

        private void Server_OnMessageReceived(ClientInfo c, string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;

            if (msg.StartsWith("CHAT|"))
            {
                string text = msg.Substring(5);
                AddLog($"[CHAT] Từ {c.Name}: {text}");
            }
            else if (msg.StartsWith("RESP|"))
            {
                AddLog($"[RESP] Từ {c.Name}: {msg.Substring(5)}");
            }
        }
        #endregion

        #region UI helpers
        private void AddLog(string text)
        {
            if (listLog.InvokeRequired)
            {
                listLog.Invoke(new Action(() => AddLog(text)));
                return;
            }

            listLog.Items.Add($"[{DateTime.Now:HH:mm:ss}] {text}");
            listLog.TopIndex = listLog.Items.Count - 1;
        }

        private void UpdateClientList()
        {
            if (lstClients.InvokeRequired)
            {
                lstClients.Invoke(new Action(UpdateClientList));
                return;
            }

            ClientInfo selectedClient = GetSelectedClient();

       
            string selectedId = selectedClient?.Id.ToString();

            lstClients.BeginUpdate();
            lstClients.Items.Clear();

            foreach (var client in server.GetClients())
            {
                string ip = (client.RemoteEndPoint != null) ? client.RemoteEndPoint.Address.ToString() : "Unknown";

                ListViewItem item = new ListViewItem(client.Name);

             
                item.SubItems.Add(ip);

               
                item.SubItems.Add(client.Id.ToString());

                item.Tag = client;

                lstClients.Items.Add(item);

                if (selectedId != null && client.Id.ToString() == selectedId)
                {
                    item.Selected = true;
                    item.EnsureVisible();
                }
            }

            lstClients.EndUpdate();

            LstClients_SelectedIndexChanged(null, null);
        }

        private ClientInfo GetSelectedClient()
        {
            
            if (lstClients.SelectedItems.Count == 0) return null;

            ListViewItem selectedItem = lstClients.SelectedItems[0];

           
            return selectedItem.Tag as ClientInfo;
        }

        

        private void LstClients_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool clientSelected = lstClients.SelectedItems.Count > 0;

            // Kiểm tra quyền Admin
            bool isAdmin = (this.role != null && this.role.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase));

            // Nút Shutdown (Chỉ Admin + Client đã chọn)
            if (btnShutdown != null)
            {
                // Chỉ bật nếu Client được chọn VÀ User có vai trò Admin
                btnShutdown.Enabled = clientSelected && isAdmin;
            }

            // Nút Restart (Chỉ Admin + Client đã chọn)
            if (btnRestart != null)
            {
                btnRestart.Enabled = clientSelected && isAdmin;
            }

            // Nút Sleep (Chỉ Admin + Client đã chọn)
            if (btnSleep != null)
            {
                btnSleep.Enabled = clientSelected && isAdmin;
            }

            // Nút Chat/Điều khiển chi tiết (Mọi người + Client đã chọn)
            if (btnChat != null)
            {
                btnChat.Enabled = clientSelected;
            }
        }
        #endregion

        #region Commands

        // Trong frmMainServer.cs (Phần Commands)

        private void BtnChat_Click(object sender, EventArgs e)
        {
            ClientInfo c = GetSelectedClient();
            if (c == null) { MessageBox.Show("Chọn client trước.", "Lỗi"); return; }

            // <<< TRUYỀN VAI TRÒ (this.role) VÀO CONSTRUCTOR CỦA frmChat >>>
           
            frmChat chat = new frmChat(server, c, this.role);
            chat.Text = $"Điều khiển: {c.Name}";
            chat.Show();
        }

        // Gửi lệnh tắt máy
        private async void BtnShutdown_Click(object sender, EventArgs e)
        {
            ClientInfo c = GetSelectedClient();
            if (c == null) { MessageBox.Show("Chọn client trước."); return; }
            // Chuỗi lệnh gửi đi: CMD|SHUTDOWN
            string command = CmdExecutor.BuildCommand("SHUTDOWN", null);
            await server.SendToClientAsync(c.Id, command);
            AddLog($"Gửi lệnh {command} tới {c.Name}");
        }

        // Gửi lệnh khởi động lại
        private async void BtnRestart_Click(object sender, EventArgs e)
        {
            ClientInfo c = GetSelectedClient();
            if (c == null) { MessageBox.Show("Chọn client trước."); return; }
            // Chuỗi lệnh gửi đi: CMD|RESTART
            string command = CmdExecutor.BuildCommand("RESTART", null);
            await server.SendToClientAsync(c.Id, command);
            AddLog($"Gửi lệnh {command} tới {c.Name}");
        }

        // Gửi lệnh ngủ/ngủ đông
        private async void BtnSleep_Click(object sender, EventArgs e)
        {
            ClientInfo c = GetSelectedClient();
            if (c == null) { MessageBox.Show("Chọn client trước."); return; }
            // Chuỗi lệnh gửi đi: CMD|SLEEP
            string command = CmdExecutor.BuildCommand("SLEEP", null);
            await server.SendToClientAsync(c.Id, command);
            AddLog($"Gửi lệnh {command} tới {c.Name}");
        }

        #endregion

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            server.Stop();
            base.OnFormClosing(e);
        }
    }
}