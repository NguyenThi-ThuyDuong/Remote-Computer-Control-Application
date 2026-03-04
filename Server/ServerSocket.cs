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
        public event Action<ClientInfo, string> OnScreenReceived; // base64 payload

        private bool running = false;

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
            running = false;
            try { listener?.Stop(); } catch { }
            foreach (var kv in clients) try { kv.Value.Tcp.Close(); } catch { }
            clients.Clear();
            Log("Server stopped.");
        }

        private async void AcceptLoop()
        {
            while (running)
            {
                try
                {
                    var tcp = await listener.AcceptTcpClientAsync();
                    var client = new ClientInfo { Tcp = tcp };
                    clients[client.Id] = client;
                    Log($"Client connected: {client.RemoteEndPoint}");
                    OnClientConnected?.Invoke(client);
                    _ = HandleClientAsync(client);
                }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex) { Log("AcceptLoop error: " + ex.Message); }
            }
        }

        private async Task HandleClientAsync(ClientInfo client)
        {
            try
            {
                // Nhận IDENTIFY
                string idMsg = await ReadMessageAsync(client.Stream);
                if (!string.IsNullOrEmpty(idMsg) && idMsg.StartsWith("IDENTIFY|"))
                {
                    client.Name = idMsg.Substring("IDENTIFY|".Length);
                    Log($"Client identified: {client.Name} ({client.RemoteEndPoint})");
                }

                // Loop nhận message
                while (running && client.Tcp.Connected)
                {
                    string msg = await ReadMessageAsync(client.Stream);
                    if (msg == null) break;

                    if (msg.StartsWith("SCREEN_DATA|"))
                    {
                        string base64 = msg.Substring("SCREEN_DATA|".Length);
                        OnScreenReceived?.Invoke(client, base64);
                        Log($"Received SCREEN_DATA from {client.Name} ({base64.Length} bytes)");
                    }
                    else
                    {
                        OnMessageReceived?.Invoke(client, msg);
                        Log($"Msg from {client.Name}: {Shorten(msg)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log("HandleClientAsync error: " + ex.Message);
            }
            finally
            {
                RemoveClient(client.Id);
            }
        }

        private void RemoveClient(Guid id)
        {
            if (clients.TryRemove(id, out var c))
            {
                try { c.Tcp.Close(); } catch { }
                Log($"Client disconnected: {c.Name} ({c.RemoteEndPoint})");
                OnClientDisconnected?.Invoke(c);
            }
        }

        public ClientInfo[] GetClients() => clients.Values.ToArray();

        public async Task<bool> SendToClientAsync(Guid clientId, string message)
        {
            if (!clients.TryGetValue(clientId, out var c)) return false;
            try
            {
                await WriteMessageAsync(c.Stream, message);
                Log($"Sent to {c.Name}: {Shorten(message)}");
                return true;
            }
            catch (Exception ex)
            {
                Log("SendToClientAsync error: " + ex.Message);
                return false;
            }
        }

        public async Task BroadcastAsync(string message)
        {
            foreach (var kv in clients) await SendToClientAsync(kv.Key, message);
        }

        #region Framing (length-prefix)
        private async Task WriteMessageAsync(NetworkStream stream, string message)
        {
            if (stream == null || !stream.CanWrite) throw new Exception("stream not writable");
            var bytes = Encoding.UTF8.GetBytes(message);
            var len = BitConverter.GetBytes(bytes.Length);
            await stream.WriteAsync(len, 0, 4);
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }

        private async Task<string> ReadMessageAsync(NetworkStream stream)
        {
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

        private void Log(string text) => OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] {text}");
        private string Shorten(string s, int n = 120) => s.Length <= n ? s : s.Substring(0, n) + "...";
    }
}
