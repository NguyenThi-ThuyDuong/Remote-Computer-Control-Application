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

namespace RemoteClient
{
    public partial class frmDangKy : Form
    {
        string connectionString = @"Data Source=HP;Initial Catalog=RemoteControlDB;Integrated Security=True";
        public frmDangKy()
        {
            InitializeComponent();
            btnDangKy.Click += BtnDangKy_Click;
            btnThoat.Click += BtnThoat_Click;
        }
        // ==============================
        //     NÚT THOÁT
        // ==============================
        private void BtnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ==============================
        //      NÚT ĐĂNG KÝ
        // ==============================
        private void BtnDangKy_Click(object sender, EventArgs e)
        {
            string username = txtUser.Text.Trim();
            string password = txtPass.Text.Trim();
            string confirm = txtConfirm.Text.Trim();

            if (username == "" || password == "" || confirm == "")
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            if (password != confirm)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!");
                return;
            }

            if (!IsValidUsername(username))
            {
                MessageBox.Show("Tên đăng nhập chỉ được chứa chữ và số!");
                return;
            }

            if (!IsValidPassword(password))
            {
                MessageBox.Show("Mật khẩu chỉ được chứa chữ cái và chữ số!");
                return;
            }

            if (KiemTraTaiKhoan(username))
            {
                MessageBox.Show("Tên đăng nhập đã tồn tại!");
                return;
            }

            // HASH MẬT KHẨU TRƯỚC KHI LƯU
            string hash = HashPassword(password);

            LuuTaiKhoan(username, hash);

            MessageBox.Show("Đăng ký thành công!");

            this.Close();
            frmDangNhap f = new frmDangNhap();
            f.Show();
        }

        private bool IsValidUsername(string username)
        {
            return username.All(c => char.IsLetterOrDigit(c));
        }
        private bool IsValidPassword(string password)
        {
            return password.All(c => char.IsLetterOrDigit(c));
        }

        // ==============================
        //     KIỂM TRA TRÙNG USERNAME
        // ==============================
        private bool KiemTraTaiKhoan(string username)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string sql = "SELECT COUNT(*) FROM NguoiDung WHERE TenDangNhap = @u";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@u", SqlDbType.NVarChar, 50).Value = username;

                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        // ==============================
        //     LƯU TÀI KHOẢN VÀO DB
        // ==============================
        private void LuuTaiKhoan(string username, string hash)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"INSERT INTO NguoiDung(TenDangNhap, MatKhauHash, VaiTro, NgayTao)
                       VALUES(@u, @p, @r, GETDATE())";

                SqlCommand cmd = new SqlCommand(sql, conn);

                cmd.Parameters.Add("@u", SqlDbType.NVarChar, 50).Value = username;

                // CHUYEN HASH HEX -> BYTE[] CHO SQL
                byte[] hashBytes = Enumerable.Range(0, hash.Length / 2)
                    .Select(i => Convert.ToByte(hash.Substring(i * 2, 2), 16))
                    .ToArray();

                cmd.Parameters.Add("@p", SqlDbType.VarBinary, 64).Value = hashBytes;

                cmd.Parameters.Add("@r", SqlDbType.NVarChar, 20).Value = "User";

                cmd.ExecuteNonQuery();
            }
        }

        // ==============================
        //      HÀM HASH SHA256
        // ==============================
        private string HashPassword(string pass)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(pass));
                return BitConverter.ToString(bytes).Replace("-", "");
            }
        }
    }
}
