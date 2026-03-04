using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    public class ClientInfo
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public TcpClient Tcp { get; set; }
        public string Name { get; set; } = "Client";
        public NetworkStream Stream => Tcp?.GetStream();
        // FIX: RemoteEndPoint đã tồn tại trong file của bạn
        public IPEndPoint RemoteEndPoint => Tcp?.Client?.RemoteEndPoint as IPEndPoint;
    }

    public class ServerSocket
    {
        private TcpListener listener;
        private readonly ConcurrentDictionary<Guid, ClientInfo> clients = new ConcurrentDictionary<Guid, ClientInfo>();

        public event Action<string> OnLog;
        public event Action<ClientInfo> OnClientConnected;
        public event Action<ClientInfo> OnClientDisconnected;
        public event Action<ClientInfo, string> OnMessageReceived;
        // FIX: OnScreenReceived đã tồn tại trong file của bạn
        public event Action<ClientInfo, string> OnScreenReceived;

        private bool running = false;

        // FIX: Phương thức Start đã tồn tại trong file của bạn
        public void Start(int port)
        {
            if (running) return;
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            running = true;
            Log($"Server started on port {port}");
            AcceptLoop();
        }

        public void Stop()
        {
            // (Giữ nguyên logic Stop của bạn)
            running = false;
            listener?.Stop();
            Log("Server stopped.");
            foreach (var client in clients.Values.ToList())
            {
                RemoveClient(client);
            }
        }

        // FIX: Thêm phương thức SendToClientAsync
        public async Task SendToClientAsync(Guid clientId, string message)
        {
            if (clients.TryGetValue(clientId, out ClientInfo client))
            {
                if (client.Tcp?.Connected == true)
                {
                    try
                    {
                        await WriteMessageAsync(client.Stream, message);
                        Log($"[SOCKET] Sent {message.Length} bytes to {client.Name}.");
                    }
                    catch (Exception ex)
                    {
                        Log($"[ERROR] Failed to send to {client.Name}: {ex.Message}");
                        RemoveClient(client);
                    }
                }
            }
            else
            {
                Log($"[WARNING] Client ID {clientId.ToString().Substring(0, 4)} not found.");
            }
        }

        // Logic ẩn (AcceptLoop, HandleClient, WriteMessageAsync, ReadMessageAsync)
        // ... (Giả định các phương thức này đã tồn tại và hoạt động trong ServerSocket.cs)

        private void AcceptLoop()
        {
            Task.Run(async () =>
            {
                while (running)
                {
                    try
                    {
                        TcpClient tcp = await listener.AcceptTcpClientAsync();
                        // Chạy mỗi client trong một Task riêng biệt
                        _ = HandleClientAsync(tcp);
                    }
                    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
                    {
                        // Lỗi khi listener.Stop() được gọi
                    }
                    catch (Exception ex)
                    {
                        if (running) Log($"[ERROR] AcceptLoop: {ex.Message}");
                    }
                }
            });
        }

        private async Task HandleClientAsync(TcpClient tcp)
        {
            var client = new ClientInfo { Tcp = tcp, Name = (tcp.Client.RemoteEndPoint as IPEndPoint)?.ToString() ?? "Unknown" };
            client.Id = Guid.NewGuid(); // Đảm bảo Id được gán nếu chưa có
            clients.TryAdd(client.Id, client);

            OnClientConnected?.Invoke(client);
            Log($"[+] Client connected: {client.Name}");

            try
            {
                while (tcp.Connected)
                {
                    string msg = await ReadMessageAsync(client.Stream);
                    if (msg == null) break;

                    // ServerSocket cần phân loại ScreenData
                    if (msg.StartsWith("SCREEN_DATA|"))
                    {
                        string base64 = msg.Substring("SCREEN_DATA|".Length);
                        OnScreenReceived?.Invoke(client, base64);
                    }
                    else
                    {
                        OnMessageReceived?.Invoke(client, msg);
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"[ERROR] Client handler error for {client.Name}: {ex.Message}");
            }
            finally
            {
                RemoveClient(client);
            }
        }

        private void RemoveClient(ClientInfo client)
        {
            if (clients.TryRemove(client.Id, out _))
            {
                client.Tcp?.Close();
                OnClientDisconnected?.Invoke(client);
                Log($"[-] Client disconnected: {client.Name}");
            }
        }

        // Phương thức nội bộ để ghi log
        private void Log(string text) => OnLog?.Invoke(text);

        #region Message Handling
        private async Task WriteMessageAsync(NetworkStream stream, string message)
        {
            // (Giữ nguyên logic WriteMessageAsync của bạn)
            if (stream == null || !stream.CanWrite) throw new Exception("stream not writable");
            var bytes = Encoding.UTF8.GetBytes(message);
            var len = BitConverter.GetBytes(bytes.Length);
            await stream.WriteAsync(len, 0, 4);
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }

        private async Task<string> ReadMessageAsync(NetworkStream stream)
        {
            // (Giữ nguyên logic ReadMessageAsync của bạn)
            if (stream == null || !stream.CanRead) return null;
            var lenBuf = new byte[4];
            int r = 0;
            while (r < 4)
            {
                int n = await stream.ReadAsync(lenBuf, r, 4 - r);
                if (n == 0) return null;
                r += n;
            }
            int len = BitConverter.ToInt32(lenBuf, 0);
            if (len <= 0) return "";
            var buf = new byte[len];
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
    }
}