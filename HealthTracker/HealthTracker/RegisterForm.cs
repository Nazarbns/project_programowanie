using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace HealthTracker
{
    public partial class RegisterForm : Form
    {
        private TextBox txtUsername, txtPassword;
        private Button btnRegister;

        public RegisterForm()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Rejestracja";
            this.Size = new Size(300, 200);
            this.StartPosition = FormStartPosition.CenterScreen;

            Label labelUsername = new Label { Text = "Nazwa użytkownika:", Location = new Point(20, 20), AutoSize = true };
            txtUsername = new TextBox { Location = new Point(150, 20), Width = 100 };

            Label labelPassword = new Label { Text = "Hasło:", Location = new Point(20, 60), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(150, 60), Width = 100, PasswordChar = '*' };

            btnRegister = new Button { Text = "Zarejestruj", Location = new Point(20, 100), Width = 100 };
            btnRegister.Click += BtnRegister_Click;

            this.Controls.Add(labelUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(labelPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnRegister);
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Wprowadź nazwę użytkownika i hasło!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=health.db;Version=3;"))
            {
                conn.Open();

                string checkUserSql = "SELECT COUNT(*) FROM Users WHERE Username = @username";
                using (SQLiteCommand checkCmd = new SQLiteCommand(checkUserSql, conn))
                {
                    checkCmd.Parameters.AddWithValue("@username", username);
                    int userExists = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (userExists > 0)
                    {
                        MessageBox.Show("Nazwa użytkownika jest już zajęta!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                string sql = "INSERT INTO Users (Username, Password) VALUES (@username, @password)";
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Rejestracja zakończona sukcesem!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
        }
    }
}
