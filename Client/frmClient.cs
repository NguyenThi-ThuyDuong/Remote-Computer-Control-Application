using client;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing; // Cần thiết cho ScreenCapture

namespace Client
{
    public partial class frmChat : Form
    {
        private ClientSocket clientSocket;
        private CancellationTokenSource screenCts;

        private bool isSleeping = false;

        private readonly int userID = 1;
        private readonly int sessionID = 100;

        // Constructor
        public frmChat(int uid, string role, int sid)
        {
            InitializeComponent();
            userID = uid;
            sessionID = sid;

            // **Đảm bảo các sự kiện được gán**
            if (btnConnect != null) btnConnect.Click += BtnConnect_Click;
            if (btnSend != null) btnSend.Click += BtnSend_Click;
            if (btnThoat != null) btnThoat.Click += BtnExit_Click;
            if (btnScreen != null) btnScreen.Click += BtnScreen_Click; 

        }

        // =============================================================
        // 0. HÀM HỖ TRỢ VÀ NÚT BẤM
        // =============================================================

        private void BtnScreen_Click(object sender, EventArgs e)
        {
            if (screenCts == null)
            {
                StartScreen();
                AddLog("Đã bật chia sẻ màn hình thủ công.");
            }
            else
            {
                StopScreen();
                AddLog("Đã tắt chia sẻ màn hình thủ công.");
            }
        }

        private void StopScreenCaptureLoop()
        {
            StopScreen();
        }

        private void EndSession()
        {
            AddLog($"[DB] Ghi nhận kết thúc phiên #{sessionID} của User ID: {userID}");
        }

        private async void BtnConnect_Click(object sender, EventArgs e)
        {
            string host = txtHost.Text.Trim();

            // Xử lý Port không hợp lệ
            if (!int.TryParse(txtPort.Text, out int port))
            {
                AddLog("Lỗi: Port không hợp lệ. Vui lòng nhập một số.");
                return;
            }

            btnConnect.Enabled = false;

            try
            {
                clientSocket?.Stop();
                clientSocket = new ClientSocket(host, port);

                clientSocket.OnMessageReceived += OnMessage;
                clientSocket.OnLog += (s) => AddLog(s); 

                AddLog($"Đang cố gắng kết nối đến {host}:{port}...");

               
                clientSocket.Start();

                
                await Task.Delay(1000);

                if (clientSocket != null)
                {
                    await clientSocket.SendMessageAsync($"IDENTIFY|USER:{userID}|PC:{Environment.MachineName}");
                    AddLog($"Đã gửi yêu cầu định danh.");
                }
            }
            catch (Exception ex)
            {
                // Trường hợp ngoại lệ hiếm gặp (ví dụ lỗi khởi tạo)
                AddLog("Lỗi kết nối nghiêm trọng: " + ex.Message);
                clientSocket?.Stop();
                btnConnect.Enabled = true;
            }
            finally
            {
               
            }
        }

        // =============================================================
        // 1. XỬ LÝ LỆNH NHẬN ĐƯỢC TỪ SERVER (OnMessage)
        // =============================================================
        private async void OnMessage(string raw)
        {
            if (lstChat.InvokeRequired)
            {
                Invoke(new Action<string>(OnMessage), raw);
                return;
            }

            // Xử lý gói tin
            if (raw.StartsWith("CHAT|"))
            {
                AddLog("Server: " + raw.Substring(5));
                return;
            }
            if (!raw.StartsWith("CMD|"))
            {
                AddLog("[RAW] " + raw);
                return;
            }

            string[] parts = raw.Split('|');
            string cmd = parts.Length > 1 ? parts[1] : "";
            string data = parts.Length > 2 ? parts[2] : "";

            try
            {
                switch (cmd)
                {
                    case "SHUTDOWN":
                        // ... (Logic SHUTDOWN) ...
                        AddLog("Đang thực thi lệnh đóng ứng dụng Client...");
                        StopScreenCaptureLoop();
                        await clientSocket.SendMessageAsync("RESP|OK|Client application closing initiated.");
                        this.Invoke(new Action(() => {
                            EndSession();
                            clientSocket?.Stop();
                            Application.Exit();
                        }));
                        break;

                    case "RESTART":
                        // ... (Logic RESTART) ...
                        AddLog("Đang khởi động lại client...");
                        StopScreenCaptureLoop();
                        await clientSocket.SendMessageAsync("RESP|OK|Client restarting...");
                        this.Invoke(new Action(() => {
                            string exePath = Application.ExecutablePath;
                            Process.Start(exePath);
                            Application.Exit();
                        }));
                        break;

                    case "SLEEP":
                        // ... (Logic SLEEP) ...
                        AddLog("Client đang chuyển sang trạng thái ngủ...");
                        StopScreenCaptureLoop();
                        isSleeping = true;
                        this.Invoke(new Action(() => {
                            this.Text = "frmChat (SLEEPING)";
                            btnSend.Enabled = false;
                            txtChat.Enabled = false;
                            if (btnScreen != null) btnScreen.Enabled = false;
                        }));
                        await clientSocket.SendMessageAsync("RESP|OK|Client is now sleeping.");
                        break;

                    case "WAKEUP":
                        // ... (Logic WAKEUP) ...
                        AddLog("Client đã thức dậy...");
                        isSleeping = false;
                        this.Invoke(new Action(() => {
                            this.Text = "frmChat";
                            btnSend.Enabled = true;
                            txtChat.Enabled = true;
                            if (btnScreen != null) btnScreen.Enabled = true;
                        }));
                        await clientSocket.SendMessageAsync("RESP|OK|Client is now awake.");
                        break;

                    case "EXEC":
                        // ... (Logic EXEC) ...
                        AddLog("🛠 Chạy lệnh CMD: " + data);
                        string res = ExecuteSystemCommand("cmd.exe", "/c " + data);
                        await clientSocket.SendMessageAsync("CHAT|Kết quả CMD:\n" + res);
                        break;

                    case "SCREEN_START":
                        AddLog("📺 Bắt đầu chia sẻ màn hình.");
                        StartScreen();
                        await clientSocket.SendMessageAsync("RESP|OK|Screen sharing started.");
                        break;

                    case "SCREEN_STOP":
                        AddLog("🛑 Dừng chia sẻ màn hình.");
                        StopScreen();
                        await clientSocket.SendMessageAsync("RESP|OK|Screen sharing stopped.");
                        break;

                    case "MESSAGE":
                        MessageBox.Show(data, "Thông báo từ Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await clientSocket.SendMessageAsync("RESP|OK|Message displayed.");
                        break;

                    default:
                        await clientSocket.SendMessageAsync("RESP|ERROR|Unknown Command: " + cmd);
                        break;
                }
            }
            catch (Exception ex)
            {
                AddLog("Lỗi thực thi lệnh: " + ex.Message);
                await clientSocket.SendMessageAsync("RESP|ERROR|Exception: " + ex.Message);
            }
        }

        // =============================================================
        // 2. HÀM THỰC THI LỆNH HỆ THỐNG (CMD)
        // =============================================================
        private string ExecuteSystemCommand(string exe, string args)
        {
            try
            {
                using (Process p = new Process
                {
                    StartInfo =
                    {
                        FileName = exe,
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                })
                {
                    p.Start();
                    string output = p.StandardOutput.ReadToEnd();
                    string error = p.StandardError.ReadToEnd();
                    p.WaitForExit(2000);

                    if (!string.IsNullOrEmpty(error)) return "Error: " + error;
                    return string.IsNullOrEmpty(output) ? "OK (No output)" : output;
                }
            }
            catch (Exception ex)
            {
                return "Exec Failed: " + ex.Message;
            }
        }

        // =============================================================
        // 3. CHỨC NĂNG CHỤP MÀN HÌNH
        // =============================================================
        private void StartScreen()
        {
            if (screenCts != null || clientSocket == null) return;

            screenCts = new CancellationTokenSource();
            var token = screenCts.Token;

            Task.Run(async () => {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        // Gọi lớp ScreenCapture bên ngoài
                        string b64 = ScreenCapture.CaptureScreenToBase64();

                        if (!string.IsNullOrEmpty(b64) && clientSocket != null)
                        {
                            await clientSocket.SendMessageAsync("SCREEN_DATA|" + b64);
                        }

                        // Delay 200ms giữa các lần chụp
                        await Task.Delay(200, token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        AddLog($"Lỗi chụp màn hình: {ex.Message}");
                    }
                }
            }, token);
        }

        private void StopScreen()
        {
            screenCts?.Cancel();
            screenCts = null;
        }

        // =============================================================
        // 4. LOG VÀ GỬI CHAT
        // =============================================================
        private async void BtnSend_Click(object sender, EventArgs e)
        {
            if (clientSocket != null && !string.IsNullOrEmpty(txtChat.Text))
            {
                await clientSocket.SendMessageAsync("CHAT|" + txtChat.Text);
                AddLog("Me: " + txtChat.Text);
                txtChat.Clear();
            }
        }

        private void AddLog(string s)
        {
            if (lstChat.InvokeRequired)
                Invoke(new Action(() => AddLog(s)));
            else
            {
                lstChat.Items.Add($"[{DateTime.Now:HH:mm:ss}] {s}");
                lstChat.TopIndex = lstChat.Items.Count - 1;
            }
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            StopScreen();
            clientSocket?.Stop();
            Application.Exit();
        }
    }
}