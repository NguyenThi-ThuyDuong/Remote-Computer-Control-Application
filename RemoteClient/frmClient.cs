using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using client;
using Client;

namespace Client
{
    public partial class frmClient : Form
    {
        private readonly ClientSocket clientSocket;
        private bool isViewingScreen = false;
        private readonly string role;

        public frmClient(ClientSocket socket, int userID, string userRole)
        {
            InitializeComponent();

            this.clientSocket = socket ?? throw new ArgumentNullException(nameof(socket));
            this.role = userRole;
            this.Text = "Điều khiển Server";

            this.Load += FrmClient_Load;

            // === SỰ KIỆN KẾT NỐI ===
            clientSocket.OnConnected += () => AddLog("[+] Đã kết nối tới Server");
            clientSocket.OnDisconnected += () => AddLog("[-] Mất kết nối Server");

            // === NHẬN TIN NHẮN TỪ SERVER ===
            clientSocket.OnMessageReceived += OnServerMessage;

            // === GÁN BUTTON ===
            btnGui.Click += BtnGui_Click;
            btnScreen.Click += BtnScreen_Click;
            btnRunCMD.Click += BtnRunCMD_Click;

            btnRestart.Click += BtnRestart_Click;
            btnShutdown.Click += BtnShutdown_Click;
            btnSleep.Click += BtnSleep_Click;

            
        }

        private void FrmClient_Load(object sender, EventArgs e)
        {
            RefreshUI();
        }

        // ======================================================
        // 1. HÀM GỬI LỆNH CHUẨN (y như frmChat)
        // ======================================================
        private async Task SendCmd(string cmd, string data = "")
        {
            string packet = string.IsNullOrEmpty(data)
                ? $"CMD|{cmd}"
                : $"CMD|{cmd}|{data}";

            await clientSocket.SendAsync(packet);
            AddLog($"[ME -> SERVER] {packet}");
        }

        // ======================================================
        // 2. NÚT GỬI LỆNH (shutdown, sleep…)
        // ======================================================

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

        private async void BtnGui_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtChat.Text))
            {
                await clientSocket.SendAsync("CHAT|" + txtChat.Text);
                AddLog("[ME] " + txtChat.Text);
                txtChat.Clear();
            }
        }

        private void BtnRunCMD_Click(object sender, EventArgs e)
        {
            string cmd = txtLenhCMD.Text.Trim();
            if (string.IsNullOrEmpty(cmd)) cmd = txtChat.Text.Trim();

            if (string.IsNullOrEmpty(cmd))
            {
                MessageBox.Show("Nhập lệnh CMD!");
                return;
            }

            SendCmd("EXEC", cmd);
            txtLenhCMD.Clear();
        }

        private void BtnScreen_Click(object sender, EventArgs e)
        {
            if (!isViewingScreen)
            {
                SendCmd("SCREEN_START");
                btnScreen.Text = "Dừng Xem";
                isViewingScreen = true;
            }
            else
            {
                SendCmd("SCREEN_STOP");
                btnScreen.Text = "Xem Màn Hình";
                isViewingScreen = false;
            }
        }

        // ======================================================
        // 3. NHẬN TIN NHẮN TỪ SERVER (giống frmChat)
        // ======================================================
        private void OnServerMessage(string msg)
        {
            if (msg.StartsWith("SCREEN|"))
            {
                string base64 = msg.Substring("SCREEN|".Length);
                UpdateScreen(base64);
                return;
            }

            AddLog("[SERVER] " + msg);
        }

        private void UpdateScreen(string base64)
        {
            try
            {
                byte[] b = Convert.FromBase64String(base64);

                using (MemoryStream ms = new MemoryStream(b))
                {
                    Image img = Image.FromStream(ms);

                    if (picScreen.InvokeRequired)
                    {
                        picScreen.Invoke(new Action(() =>
                        {
                            if (picScreen.Image != null) picScreen.Image.Dispose();
                            picScreen.Image = new Bitmap(img);
                        }));
                    }
                    else
                    {
                        if (picScreen.Image != null) picScreen.Image.Dispose();
                        picScreen.Image = new Bitmap(img);
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog("Lỗi ảnh màn hình: " + ex.Message);
            }
        }

        // ======================================================
        // 4. LOG + PERMISSION + UI
        // ======================================================
        private void AddLog(string s)
        {
            if (lstChat.InvokeRequired)
            {
                lstChat.Invoke(new Action(() => AddLog(s)));
            }
            else
            {
                lstChat.Items.Add($"[{DateTime.Now:HH:mm:ss}] {s}");
                lstChat.TopIndex = lstChat.Items.Count - 1;
            }
        }

       

        private void RefreshUI()
        {
            bool connected = clientSocket.IsConnected;

            btnGui.Enabled = connected;
            btnScreen.Enabled = connected && btnScreen.Enabled;
            btnRunCMD.Enabled = connected && btnRunCMD.Enabled;
            btnRestart.Enabled = connected && btnRestart.Enabled;
            btnShutdown.Enabled = connected && btnShutdown.Enabled;
            btnSleep.Enabled = connected && btnSleep.Enabled;
        }
    }
}
