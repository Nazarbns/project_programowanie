using System;
using System.Data.SQLite;
using System.Windows.Forms;

namespace HealthTracker
{
    public partial class LoginForm : Form
    {
        public int UserId { get; private set; }  // Добавлено свойство для хранения ID пользователя

        public LoginForm()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            RegisterForm registerForm = new RegisterForm();
            registerForm.ShowDialog();
        }

        private void BtnLogin_Click(object sender, EventArgs e)
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
                string sql = "SELECT Id FROM Users WHERE Username = @username AND Password = @password";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    object result = cmd.ExecuteScalar();

                    if (result != null && int.TryParse(result.ToString(), out int userId))
                    {
                        UserId = userId; // Сохраняем ID пользователя
                        MessageBox.Show("Logowanie zakończone!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Nieprawidłowa nazwa użytkownika lub hasło!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }
    }
}
