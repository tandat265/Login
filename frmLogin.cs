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

namespace BTGK
{
    public partial class frmLogin : Form
    {
        string strCon = @"Data Source=DESKTOP-1A0DHPA;Initial Catalog=CSATTT;Integrated Security=True";
        SqlConnection sqlCon = null;
        SqlCommand command;
        public frmLogin()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (txtUsername.Text.ToString() == "")
            {
                MessageBox.Show("Chưa nhập tên đăng nhập!!!");
                txtUsername.Focus();
            }
            else if (txtPassword.Text.ToString() == "")
            {
                MessageBox.Show("Chưa nhập mật khẩu!!!");
                txtPassword.Focus();
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
                        user.TimesLogin = (int)reader["TimesLogin"];
                        lstUser.Add(user);
                    }
                    reader.Close();
                    command.Dispose();
                    // kiểm tra tên đăng nhập đã tồn tại hay chưa
                    foreach (User user in lstUser)
                    {
                        if (txtUsername.Text.ToString().CompareTo(user.Name) == 0)
                        {
                            trangThai = true;
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
							if (user.TimesLogin == 5)
                            {
                                MessageBox.Show("Bạn đã nhập sai mật khẩu 5 lần. Bạn đã hết quyền đăng nhập");
                            }
                            else if (passMd5 == user.Password)
                            {
                                MessageBox.Show("Bạn đã đăng nhập thành công");
                                this.Hide();
                                frmHome home = new frmHome();
                                home.StartPosition = FormStartPosition.CenterParent;
                                home.ShowDialog();
                            }
                            else
                            {
                                string updateQuery = "UPDATE [CSATTT].[dbo].[Users] SET TimesLogin = @NewValue WHERE Name = @RecordID";
                                SqlCommand command = new SqlCommand(updateQuery, sqlCon);
                                command.Parameters.AddWithValue("@NewValue", (user.TimesLogin + 1)); // newValue là giá trị mới bạn muốn cập nhật
                                command.Parameters.AddWithValue("@RecordID", user.Name); // recordID là ID của bản ghi bạn muốn cập nhật
                                command.ExecuteNonQuery();
                                MessageBox.Show("Bạn đã nhập sai mật khẩu. Bạn còn " + (5 - user.TimesLogin) + " đăng nhập sai.");
                            }
                            break;
                        }

                    }
                    if (trangThai == false)
                    {
                        MessageBox.Show("Tên đăng nhập không tồn tại tồn tại!!!");
                        txtUsername.Clear();
                        txtPassword.Clear();
                        trangThai = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtUsername.Clear();
            txtPassword.Clear();
            txtUsername.Focus();
        }

        private void checkbxShowPas_CheckedChanged(object sender, EventArgs e)
        {
            // hiện mật khẩu
            if (checkbxShowPas.Checked)
            {
                txtPassword.PasswordChar = '\0';
            }
            else
            {
                txtPassword.PasswordChar = '*';
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {
            // chuyển qua form đăng ký
            this.Hide();
            frmRegister register = new frmRegister();
            register.StartPosition = FormStartPosition.CenterParent;
            register.ShowDialog();
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            this.StartPosition = FormStartPosition.CenterParent;
            txtUsername.Focus();
        }
    }
}
