using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using server;
using RemoteServer;

// Removed: using server.Network;

namespace Server
{
    // Giả định lớp ScreenViewerForm tồn tại trong namespace Server hoặc được referenced
    public partial class FrmServer : Form
    {
        private readonly ServerSocket server = new ServerSocket();
        private ScreenViewerForm screenViewer;
        private int phienID;

        private ClientInfo SelectedClient => lstClients.SelectedItem as ClientInfo;

        // FIX: Xóa các tham số không sử dụng (userID, role, phienID)
        public FrmServer(int userID, string role)
        {
            InitializeComponent();

            btnStart.Click += (s, e) =>
            {
                if (int.TryParse(txtPort.Text.Trim(), out int port))
                {
                    server.Start(port);
                }
            };
            btnStop.Click += BtnStop_Click;
            btnSend.Click += BtnSend_Click;

            // Gán sự kiện
            server.OnLog += AddLog;
            server.OnClientConnected += Server_OnClientConnected;
            server.OnClientDisconnected += Server_OnClientDisconnected;
            server.OnMessageReceived += Server_OnMessageReceived;
            server.OnScreenReceived += Server_OnScreenReceived;

            lstClients.DisplayMember = "Name";
            txtPort.Text = "11000";
        }

        public FrmServer(int userID, string role, int phienID) : this(userID, role)
        {
            this.phienID = phienID;
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            server.Stop();
            AddLog("[SERVER] Server đã dừng.");
            lstClients.Invoke(() => lstClients.Items.Clear());
        }

        // ===================================
        // 1. SERVER GỬI TIN NHẮN CHAT CHO CLIENT
        // ===================================
        private async void BtnSend_Click(object sender, EventArgs e)
        {
            if (SelectedClient != null && !string.IsNullOrEmpty(txtMessage.Text))
            {
                // CmdExecutor đã nằm trong namespace 'server'
                string chatMessage = CmdExecutor.BuildCommand("CHAT", $"[SERVER]: {txtMessage.Text}");

                await server.SendToClientAsync(SelectedClient.Id, chatMessage);
                AddLog($"[ME (Chat) -> {SelectedClient.Name}] {txtMessage.Text}");
                txtMessage.Clear();
            }
        }

        // =========================================================
        // 2. NHẬN LỆNH TỪ CLIENT VÀ THỰC THI ẨN TRÊN SERVER
        // =========================================================
        private void Server_OnMessageReceived(ClientInfo client, string message)
        {
            string parsedMessage = CmdExecutor.ParseClientMessage(message);
            AddLog($"[{client.Name}] {parsedMessage}");

            if (!message.StartsWith("CMD|")) return;

            string[] parts = message.Split('|');
            string cmd = parts.Length > 1 ? parts[1].ToUpper() : "";
            string data = parts.Length > 2 ? parts[2] : "";

            AddLog($"[COMMAND FROM {client.Name}] Lệnh: {cmd} (EXECUTE SILENTLY ON SERVER)");

            // Khai báo StartInfo chung để thực thi ẩn (Silent Execution)
            ProcessStartInfo CreateHiddenProcess(string fileName, string args)
            {
                return new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true    // Tắt cửa sổ
                };
            }

            switch (cmd)
            {
                case "SHUTDOWN":
                    AddLog("[SERVER] Thực hiện lệnh SHUTDOWN (Silent)...");
                    Process.Start(CreateHiddenProcess("shutdown", "/s /t 0"));
                    break;
                case "RESTART":
                    AddLog("[SERVER] Thực hiện lệnh RESTART (Silent)...");
                    Process.Start(CreateHiddenProcess("shutdown", "/r /t 0"));
                    break;
                case "EXEC":
                    if (!string.IsNullOrEmpty(data))
                    {
                        AddLog($"[SERVER] Thực thi CMD (Silent): {data}");
                        try
                        {
                            // Thực thi CMD trên máy Server (ẩn)
                            Process.Start(CreateHiddenProcess("cmd.exe", "/C " + data));
                        }
                        catch (Exception ex)
                        {
                            AddLog("[SERVER] Lỗi thực thi CMD: " + ex.Message);
                        }
                    }
                    break;

                // Các lệnh Screen Start/Stop (Client báo hiệu)
                case "SCREEN_START":
                    this.Invoke(() =>
                    {
                        if (screenViewer == null || screenViewer.IsDisposed)
                        {
                            screenViewer = new ScreenViewerForm();
                            screenViewer.Text = $"Remote Screen: {client.Name}";
                            screenViewer.Show();
                        }
                    });
                    break;
                case "SCREEN_STOP":
                    if (screenViewer != null && !screenViewer.IsDisposed && screenViewer.Text.Contains(client.Name))
                    {
                        screenViewer.Invoke(() => screenViewer.Close());
                    }
                    break;
                default:
                    AddLog("[SERVER] Lệnh từ Client không xác định: " + cmd);
                    break;
            }
        }

        // =========================================================
        // 3. NHẬN DỮ LIỆU MÀN HÌNH TỪ CLIENT
        // =========================================================
        private void Server_OnScreenReceived(ClientInfo client, string base64)
        {
            if (screenViewer != null && !screenViewer.IsDisposed && client.Id == SelectedClient?.Id)
            {
                screenViewer.DisplayScreen(base64);
            }
        }

        // ===================================
        // XỬ LÝ KẾT NỐI & LOG
        // ===================================
        private void Server_OnClientConnected(ClientInfo client)
        {
            AddLog($"[+] Client connected: {client.Name} ({client.RemoteEndPoint})");
            lstClients.Invoke(() => lstClients.Items.Add(client));
        }

        private void Server_OnClientDisconnected(ClientInfo client)
        {
            AddLog($"[-] Client disconnected: {client.Name}");
            lstClients.Invoke(() =>
            {
                var itemToRemove = lstClients.Items.Cast<ClientInfo>().FirstOrDefault(c => c.Id == client.Id);
                if (itemToRemove != null) lstClients.Items.Remove(itemToRemove);
            });

            if (screenViewer != null && !screenViewer.IsDisposed && screenViewer.Text.Contains(client.Name))
            {
                screenViewer.Invoke(() => screenViewer.Close());
            }
        }

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
    }

    // Extension method cho Invoke
    public static class ControlExtensions
    {
        public static void Invoke(this Control c, Action a)
        {
            if (c.InvokeRequired) c.Invoke(a);
            else a();
        }
    }
}