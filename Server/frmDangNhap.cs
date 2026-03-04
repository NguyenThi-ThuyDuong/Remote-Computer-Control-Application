using server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class frmDangNhap : Form
    {
        string connectionString = @"Data Source=HP;Initial Catalog=RemoteControlDB;Integrated Security=True";
        private readonly ServerSocket server;
        public frmDangNhap()
        {
            InitializeComponent();
            btnDangNhap.Click += BtnDangNhap_Click;
        }
        private void BtnDangNhap_Click(object sender, EventArgs e)
        {
            string tk = txtTK.Text.Trim();
            string mk = txtPass.Text.Trim();

            if (tk == "" || mk == "")
            {
                MessageBox.Show("Vui lòng nhập tài khoản và mật khẩu!");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Gọi sp_DangNhap lấy thông tin user
                SqlCommand cmd = new SqlCommand("sp_DangNhap", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenDangNhap", tk);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Tài khoản không tồn tại!");
                    return;
                }

                DataRow row = dt.Rows[0];
                byte[] pwdDB = (byte[])row["MatKhauHash"];
                byte[] pwdUser = HashSHA256(mk);

                if (!CompareHash(pwdDB, pwdUser))
                {
                    MessageBox.Show("Sai mật khẩu!");
                    return;
                }

                // Đăng nhập thành công
                int userID = Convert.ToInt32(row["NguoiDungID"]);
                string role = row["VaiTro"].ToString();

                // Tạo phiên kết nối mới (PhienKetNoi)
                int mayChuID = 1; 
                int phienID = TaoPhien(conn, userID, mayChuID);

                MessageBox.Show("Đăng nhập thành công!");

                // Chuyển sang form ClientMain
                this.Hide();
               frmMainServer f = new frmMainServer( userID, role);

                f.Show();
            }
        }

        // ================= HASH ==================
        public static byte[] HashSHA256(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            }
        }

        private bool CompareHash(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i]) return false;
            return true;
        }

        // =============== TẠO PHIÊN =================
        private int TaoPhien(SqlConnection conn, int userID, int mayChuID)
        {
            SqlCommand cmd = new SqlCommand("sp_TaoPhien", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@NguoiDungID", userID);
            cmd.Parameters.AddWithValue("@MayChuID", mayChuID);

            object result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);
        }

        private void btnDangKy_Click_1(object sender, EventArgs e)
        {
            frmDangKy f = new frmDangKy();
            f.ShowDialog();
        }
    }
}
