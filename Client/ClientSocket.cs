using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics; // Cần thêm để sử dụng Process

namespace client
{
    public class ClientSocket
    {
        private readonly string serverIP;
        private readonly int serverPort;

        private TcpClient tcp;
        private NetworkStream stream;
        private CancellationTokenSource cts;
      

        public event Action<string> OnLog;
        public event Action<string> OnMessageReceived;

        public ClientSocket(string ip, int port)
        {
            serverIP = ip;
            serverPort = port;
        }

        // =======================
        // START CONNECTION
        // =======================
        public void Start()
        {
            cts = new CancellationTokenSource();
            // Chạy kết nối trong một Task riêng để không chặn UI
            Task.Run(() => ConnectLoopAsync(cts.Token));
        }

        // =======================
        // STOP CONNECTION
        // =======================
        public void Stop()
        {
            try
            {
                cts?.Cancel(); // Yêu cầu dừng vòng lặp
                stream?.Close();
                tcp?.Close();
                OnLog?.Invoke("Socket closed.");
            }
            catch { }
        }

        // =======================
        // CONNECT LOOP
        // =======================
        private async Task ConnectLoopAsync(CancellationToken token)
        {
            try
            {
                tcp = new TcpClient();
                OnLog?.Invoke($"Attempting to connect to {serverIP}:{serverPort}");

                await tcp.ConnectAsync(serverIP, serverPort);
                stream = tcp.GetStream();

                OnLog?.Invoke("Connection successful.");

                while (!token.IsCancellationRequested)
                {
                    // Đọc message theo giao thức Length-Prefix
                    string message = await ReadMessageAsync(stream);

                    if (string.IsNullOrEmpty(message))
                    {
                        // Server đã đóng kết nối
                        OnLog?.Invoke("Server disconnected.");
                        break;
                    }

                    OnMessageReceived?.Invoke(message);
                }
            }
            catch (TaskCanceledException)
            {
                OnLog?.Invoke("Connection loop cancelled.");
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"Connection error: {ex.Message}");
            }
            finally
            {
                Stop();
            }
        }

        // =======================
        // SEND MESSAGE ASYNC
        // =======================
        public async Task SendMessageAsync(string message)
        {
            if (stream == null || !stream.CanWrite)
            {
                OnLog?.Invoke("Cannot send message: Stream not writable.");
                return;
            }

            try
            {
                await WriteMessageAsync(stream, message);
                // OnLog?.Invoke($"Sent: {Shorten(message)}");
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"Send failed: {ex.Message}");
            }
        }

        // =======================
        // LENGTH-PREFIX MESSAGE
        // (Write)
        // =======================
        private async Task WriteMessageAsync(NetworkStream stream, string message)
        {
            if (stream == null || !stream.CanWrite)
                throw new Exception("stream not writable");

            var bytes = Encoding.UTF8.GetBytes(message);
            var len = BitConverter.GetBytes(bytes.Length);

            // 1. Gửi độ dài (4 bytes)
            await stream.WriteAsync(len, 0, 4);
            // 2. Gửi dữ liệu
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }

        // =======================
        // LENGTH-PREFIX MESSAGE
        // (Read)
        // =======================
        private async Task<string> ReadMessageAsync(NetworkStream stream)
        {
            if (stream == null || !stream.CanRead) return null;

            // 1. Đọc độ dài (4 bytes)
            var lenBuf = new byte[4];
            int r = 0;
            while (r < 4)
            {
                int n = await stream.ReadAsync(lenBuf, r, 4 - r);
                if (n == 0) return null; // Mất kết nối
                r += n;
            }

            int len = BitConverter.ToInt32(lenBuf, 0);
            if (len <= 0) return "";

            // 2. Đọc dữ liệu
            var buf = new byte[len];
            int read = 0;
            while (read < len)
            {
                int n = await stream.ReadAsync(buf, read, len - read);
                if (n == 0) return null; // Mất kết nối
                read += n;
            }

            return Encoding.UTF8.GetString(buf, 0, len);
        }

        // =======================
        // LOG helper
        // =======================
        private string Shorten(string s, int n = 80)
        {
            if (s.Length <= n) return s;
            return s.Substring(0, n) + "...";
        }
    }
}