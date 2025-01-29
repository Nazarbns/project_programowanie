using System;
using System.Windows.Forms;

namespace HealthTracker
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Создаем объект главной формы
            MainForm mainForm = new MainForm(0); // Временный userId

            // **Вызов метода для создания базы данных**
            mainForm.InitializeDatabase();

            // Открываем окно логина
            LoginForm loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                int LoggedInUserId = loginForm.UserId; // Убедитесь, что есть свойство UserId
                Application.Run(new MainForm(LoggedInUserId));
            }
        }


    }
}
