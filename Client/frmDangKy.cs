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

namespace Client
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


            // VALIDATION
            if (username == "" || password == "" || confirm == "")
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }
            if (!IsValidUsername(username))
            {
                MessageBox.Show("Tên đăng nhập chỉ được chứa chữ cái và số (không được chứa ký tự đặc biệt)!");
                return;
            }

            // 2. Kiểm tra password hợp lệ
            if (!IsPasswordValid(password))
            {
                MessageBox.Show("Mật khẩu chỉ được chứa chữ cái và số (không được chứa ký tự đặc biệt)!");
                return;
            }
            if (password != confirm)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!");
                return;
            }


            // 1. kiểm tra tài khoản tồn tại
            if (KiemTraTaiKhoan(username))
            {
                MessageBox.Show("Tên đăng nhập đã tồn tại!");
                return;
            }

            // 2. Mã hóa mật khẩu
            string hash = HashPassword(password);

            // 3. Lưu vào CSDL
            LuuTaiKhoan(username, hash);

            MessageBox.Show("Đăng ký thành công!");

            this.Close(); // quay về màn hình đăng nhập
            frmDangNhap f = new frmDangNhap();
            f.Show();
        }
        // ==============================
        //     KIỂM TRA KÍ TỰ ĐẶC BIỆT
        // ==============================
        private bool IsPasswordValid(string password)
        {
            // CHỈ CHO PHÉP: chữ + số
            foreach (char c in password)
            {
                if (!char.IsLetterOrDigit(c))
                    return false;
            }
            return true;
        }
        private bool IsValidUsername(string username)
        {
            return username.All(c => char.IsLetterOrDigit(c));
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



                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@u", username);

                int count = (int)cmd.ExecuteScalar();
                return count > 0;
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

                // Username
                cmd.Parameters.Add("@u", SqlDbType.NVarChar, 50).Value = username;

                // Convert chuỗi HEX hash → byte[] để lưu đúng VARBINARY(64)
                byte[] hashBytes = Enumerable.Range(0, hash.Length / 2)
                    .Select(x => Convert.ToByte(hash.Substring(x * 2, 2), 16))
                    .ToArray();

                cmd.Parameters.Add("@p", SqlDbType.VarBinary, 64).Value = hashBytes;

                // Vai trò
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
