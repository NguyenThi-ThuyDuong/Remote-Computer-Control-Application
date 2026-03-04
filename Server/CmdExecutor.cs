using System;

namespace server.Network
{
    public static class CmdExecutor
    {
        /// <summary>
        /// Phân tích thông điệp từ client gởi về server
        /// (áp dụng cho giao thức length-prefix)
        /// </summary>
        public static string ParseClientMessage(string raw)
        {
            try
            {
                if (string.IsNullOrEmpty(raw))
                    return "";

                // RESP|OK|Shutdown completed
                if (raw.StartsWith("RESP|"))
                {
                    string content = raw.Substring("RESP|".Length);
                    return "[CLIENT RESPONSE] " + content;
                }

                // CHAT|text message
                if (raw.StartsWith("CHAT|"))
                {
                    string content = raw.Substring("CHAT|".Length);
                    return "[CLIENT CHAT] " + content;
                }

                // LOG|client internal log
                if (raw.StartsWith("LOG|"))
                {
                    string content = raw.Substring("LOG|".Length);
                    return "[CLIENT LOG] " + content;
                }

                // Unknown protocol
                return "[RAW] " + raw;
            }
            catch (Exception ex)
            {
                return "[Parse error] " + ex.Message;
            }
        }

        /// <summary>
        /// Dùng trong UI để gửi lệnh standard đến client
        /// Server không tự thực thi command như shutdown
        /// </summary>
        public static string BuildCommand(string cmd, string data)
        {
            // Ví dụ:
            // CMD|SHUTDOWN
            // CMD|MESSAGE|Hello client
            try
            {
                if (string.IsNullOrEmpty(cmd))
                    return "CMD|EMPTY";

                if (string.IsNullOrEmpty(data))
                    return "CMD|" + cmd;

                return "CMD|" + cmd + "|" + data;
            }
            catch
            {
                return "CMD|INVALID";
            }
        }
    }
}
