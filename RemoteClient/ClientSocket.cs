using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace client
{
    public class ClientSocket
    {
        private TcpClient tcp;
        private NetworkStream Stream => tcp?.GetStream();

        public bool IsConnected => tcp?.Connected ?? false;

        private bool running = false;

        public event Action<string> OnLog;
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnMessageReceived;
        public event Action<string> OnScreenReceived; // base64

        public async Task<bool> ConnectAsync(string host, int port, string clientName)
        {
            try
            {
                tcp = new TcpClient();
                await tcp.ConnectAsync(host, port);

                running = true;
                Log($"Connected to {host}:{port}");
                OnConnected?.Invoke();

                // Gửi IDENTIFY
                await SendAsync($"IDENTIFY|{clientName}");

                _ = ReceiveLoop();

                return true;
            }
            catch (Exception ex)
            {
                Log("Connect error: " + ex.Message);
                return false;
            }
        }

        public void Disconnect()
        {
            running = false;
            try { tcp?.Close(); } catch { }
            Log("Disconnected.");
            OnDisconnected?.Invoke();
        }

        private async Task ReceiveLoop()
        {
            try
            {
                while (running && tcp.Connected)
                {
                    string msg = await ReadMessageAsync(Stream);
                    if (msg == null) break;

                    if (msg.StartsWith("SCREEN_DATA|"))
                    {
                        string base64 = msg.Substring("SCREEN_DATA|".Length);
                        OnScreenReceived?.Invoke(base64);
                        Log($"Received SCREEN_DATA ({base64.Length} bytes)");
                    }
                    else
                    {
                        OnMessageReceived?.Invoke(msg);
                        Log($"Received: {Shorten(msg)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log("ReceiveLoop error: " + ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }

        public async Task<bool> SendAsync(string message)
        {
            try
            {
                await WriteMessageAsync(Stream, message);
                Log("Sent: " + Shorten(message));
                return true;
            }
            catch (Exception ex)
            {
                Log("SendAsync error: " + ex.Message);
                return false;
            }
        }

        #region Framing (length-prefix)

        private async Task WriteMessageAsync(NetworkStream stream, string message)
        {
            if (stream == null || !stream.CanWrite) throw new Exception("Stream not writable");

            byte[] bytes = Encoding.UTF8.GetBytes(message);
            byte[] len = BitConverter.GetBytes(bytes.Length);

            await stream.WriteAsync(len, 0, 4);
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }

        private async Task<string> ReadMessageAsync(NetworkStream stream)
        {
            if (stream == null || !stream.CanRead) return null;

            byte[] lenBuf = new byte[4];
            int r = 0;

            while (r < 4)
            {
                int n = await stream.ReadAsync(lenBuf, r, 4 - r);
                if (n == 0) return null;
                r += n;
            }

            int len = BitConverter.ToInt32(lenBuf, 0);
            if (len <= 0) return "";

            byte[] buf = new byte[len];
            int read = 0;

            while (read < len)
            {
                int n = await stream.ReadAsync(buf, read, len - read);
                if (n == 0) return null;
                read += n;
            }

            return Encoding.UTF8.GetString(buf, 0, len);
        }

        #endregion

        private void Log(string msg) => OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] {msg}");

        private string Shorten(string s, int n = 80) =>
            s.Length <= n ? s : s.Substring(0, n) + "...";
    }
}
