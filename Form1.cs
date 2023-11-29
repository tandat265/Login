using Google.Cloud.Translation.V2;
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
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace BTGK
{
    public partial class frmRegister : Form
    {
        string strCon = @"Data Source=DESKTOP-1A0DHPA;Initial Catalog=CSATTT;Integrated Security=True";
        SqlConnection sqlCon = null;
        SqlCommand command;
        public frmRegister()
        {
            InitializeComponent();
            txtUsername.Focus();
        }
        // Hàm chuyển đổi từ chuỗi hex sang byte array
        public static byte[] StringToByteArray(string hex)
        {
            int length = hex.Length;
            byte[] bytes = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
        // Chuỗi SHA-256 đầu vào
        //string sha256String = "6b86b273ff34fce19d6b804eff5a3f5736039484e885aa2e1d5f9cc9f6ed252a9";
        // Chuyển đổi từ chuỗi SHA-256 sang byte array
        //byte[] sha256Bytes = StringToByteArray(sha256String);
        // Chuyển đổi từ byte array sang chuỗi kí tự
        //string originalString = Encoding.UTF8.GetString(sha256Bytes);
        private void button1_Click(object sender, EventArgs e)
        {
            if (txtUsername.Text.ToString() == "")
            {
                MessageBox.Show("Chưa nhập tên đăng nhập!!!");
                txtUsername.Focus();
            }
            else if (txtPassword.Text.ToString() == "")
            {
                MessageBox.Show("Chưa nhập mật khẩu!!!");
                txtComPassword.Clear();
                txtPassword.Focus();
            }  
            else if (txtComPassword.Text.ToString() == "")
            {
                MessageBox.Show("Chưa xác nhận mật khẩu!!!");
                txtComPassword.Focus();
            }
            else if (txtPassword.Text.ToString() != txtComPassword.Text.ToString())
            {
                MessageBox.Show("Xác nhận mật khẩu không đúng!!!");
                txtComPassword.Clear();
                txtComPassword.Focus();
            }
            else if (txtPassword.Text.Length < 8)
            {
                MessageBox.Show("Mật khẩu phải dài hơn ít nhất 8 ký tự!!!");
                txtPassword.Clear();
                txtComPassword.Clear();
                txtComPassword.Focus();
            }
            else
            {
                try
                {
                    List<User> lstUser = new List<User>();
                    sqlCon = new SqlConnection(strCon);
                    bool trangThai = false;
                    if (sqlCon.State == ConnectionState.Closed)
                    {
                        sqlCon.Open();
                    }
                    // Add data từ SQL vào List<User>
                    string query = "SELECT * FROM [CSATTT].[dbo].[Users]";
                    command = new SqlCommand(query, sqlCon);
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        User user = new User();
                        user.Name = reader["Name"].ToString();
                        user.Password = reader["Password"].ToString(); 
                        lstUser.Add(user);
                    }
                    reader.Close();
                    command.Dispose();
                    // kiểm tra tên đăng nhập đã tồn tại hay chưa
                    foreach (User user in lstUser)
                    {
                        if (txtUsername.Text.ToString().CompareTo(user.Name) == 0)
                        {
                            MessageBox.Show("Tên đăng nhập đã tồn tại!!!");
                            txtUsername.Clear();
                            txtPassword.Clear();
                            txtComPassword.Clear();
                            trangThai = true;
                            break;
                        }

                    }
					// băm password 
					StringBuilder stringBuilder = new StringBuilder();
					using (MD5 md5 = MD5.Create())
                    {
                        byte[] inputBytes = Encoding.UTF8.GetBytes(txtPassword.Text.ToString());
                        byte[] bytes = md5.ComputeHash(inputBytes);

                        for (int i = 0; i < bytes.Length; i++)
                        {
                            stringBuilder.Append(bytes[i].ToString("x2")); // Convert byte to hexadecimal representation
                        }
                    }
                    string passMd5 = stringBuilder.ToString();
					// thêm tài khoản vào CSDL
					string insertQuery = "INSERT INTO [CSATTT].[dbo].[Users] (Name, Password, TimesLogin) VALUES (@Name, @Password, @TimesLogin)";
                    SqlCommand insertCommand = new SqlCommand(insertQuery, sqlCon);
                    insertCommand.Parameters.AddWithValue("@Name", txtUsername.Text.ToString());
                    insertCommand.Parameters.AddWithValue("@Password", passMd5);
                    insertCommand.Parameters.AddWithValue("@TimesLogin", 0);
                    insertCommand.ExecuteNonQuery();
                    insertCommand.Dispose();
                    MessageBox.Show("Tài khoản đã được đăng ký thành công");
                    // chuyển qua form đăng nhập
                    this.Hide();
                    frmLogin login = new frmLogin();
                    login.StartPosition = FormStartPosition.CenterParent;
                    login.ShowDialog();
                }              
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtUsername.Clear();
            txtPassword.Clear();
            txtComPassword.Clear();
            txtUsername.Focus();
        }

        private void checkbxShowPas_CheckedChanged(object sender, EventArgs e)
        {
            // hiện mật khẩu
            if (checkbxShowPas.Checked)
            {
                txtPassword.PasswordChar = '\0';
                txtComPassword.PasswordChar = '\0';
            }
            else
            {
                txtPassword.PasswordChar = '*';
                txtComPassword.PasswordChar = '*';
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {
            // chuyển qua form đăng nhập
            this.Hide();
            frmLogin login = new frmLogin();
            login.StartPosition = FormStartPosition.CenterParent;
            login.ShowDialog();
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            // đánh giá mức độ mạnh yếu của mật khẩu và đưa ra lời khuyên
            string pass = txtPassword.Text.ToString();
            txtDanhGiaPass.Visible = false;
            suggetion.Visible = false;
            if (pass != "")
            {
                var result = Zxcvbn.Core.EvaluatePassword(pass);
                int score = result.Score;
                string feedback = string.Join(", ", result.Feedback.Suggestions); // Gợi ý để cải thiện mật khẩu        
                if (pass.Length < 8)
                {
                    txtDanhGiaPass.Text = "Mật khẩu quá ngắn";
                    suggetion.Text = "Gợi ý: Mật khẩu phải dài ít nhất 8 ký tự";
                    txtDanhGiaPass.Visible = true;
                    suggetion.Visible = true;
                }
                else if (score == 0)
                {
                    txtDanhGiaPass.Text = "Mật khẩu rất yếu";
                    suggetion.Text = "Gợi ý: " + feedback;
                    txtDanhGiaPass.Visible = true;
                    suggetion.Visible = true;
                }
                else if (score == 1)
                {
                    txtDanhGiaPass.Text = "Mật khẩu yếu";
                    suggetion.Text = "Gợi ý: " + feedback;
                    txtDanhGiaPass.Visible = true;
                    suggetion.Visible = true;
                }
                else if (score == 2)
                {
                    txtDanhGiaPass.Text = "Mật khẩu trung bình";
                    suggetion.Text = "Gợi ý: " + feedback;
                    txtDanhGiaPass.Visible = true;
                    suggetion.Visible = true;
                }
                else if (score == 3)
                {
                    txtDanhGiaPass.Text = "Mật khẩu mạnh";
                    suggetion.Text = "Gợi ý: " + feedback;
                    txtDanhGiaPass.Visible = true;
                    suggetion.Visible = true;
                }
                else if (score == 4)
                {
                    txtDanhGiaPass.Text = "Mật khẩu rất mạnh";
                    suggetion.Text = "Gợi ý: " + feedback;
                    txtDanhGiaPass.Visible = true;
                    suggetion.Visible = true;
                }
            }
            
        }

        private void frmRegister_Load(object sender, EventArgs e)
        {
            this.StartPosition = FormStartPosition.CenterParent;
            txtUsername.Focus();
        }
        
    }
}
