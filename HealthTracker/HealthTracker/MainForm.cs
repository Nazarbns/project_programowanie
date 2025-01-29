using System;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace HealthTracker
{
    public partial class MainForm : Form
    {
        private readonly int UserId;
        private Label labelWaga, labelCisnienie, labelPuls, titleLabel;
        private TextBox txtWaga, txtCisnienie, txtPuls;
        private Button btnZapisz, btnWyczysc, btnEksport, btnUsun, btnSearch;
        private ComboBox comboFilter;
        private DataGridView dataGridZdrowie;
        private Panel headerPanel;

        public MainForm(int userId)
        {
            InitializeComponent();
            this.UserId = userId;
            InitializeDatabase();
            InitializeUI();
            LoadDataFromDatabase();
        }

        public void InitializeDatabase()
        {
            string dbPath = "health.db";
            string connectionString = $"Data Source={dbPath};Version=3;";

            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sqlUsers = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT UNIQUE NOT NULL,
                Password TEXT NOT NULL
            );";
                new SQLiteCommand(sqlUsers, conn).ExecuteNonQuery();

                string sqlHealth = @"
            CREATE TABLE IF NOT EXISTS HealthData (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                Date TEXT NOT NULL,
                Weight REAL NOT NULL,
                Pressure TEXT NOT NULL,
                Pulse INTEGER NOT NULL,
                FOREIGN KEY(UserId) REFERENCES Users(Id)
            );";
                new SQLiteCommand(sqlHealth, conn).ExecuteNonQuery();
            }
        }


        private void InitializeUI()
        {
            // Заголовок
            headerPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.LightBlue };
            titleLabel = new Label
            {
                Text = "Health Tracker",
                Font = new Font("Arial", 18, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            headerPanel.Controls.Add(titleLabel);
            this.Controls.Add(headerPanel);

            labelWaga = new Label { Text = "Waga (kg):", Location = new Point(20, 70), AutoSize = true };
            labelCisnienie = new Label { Text = "Ciśnienie (mmHg):", Location = new Point(20, 110), AutoSize = true };
            labelPuls = new Label { Text = "Puls (bpm):", Location = new Point(20, 150), AutoSize = true };

            txtWaga = new TextBox { Location = new Point(150, 70), Width = 120 };
            txtCisnienie = new TextBox { Location = new Point(150, 110), Width = 120 };
            txtPuls = new TextBox { Location = new Point(150, 150), Width = 120 };

            btnZapisz = CreateStyledButton("Zapisz", new Point(300, 70), Color.LightGreen);
            btnZapisz.Click += BtnZapisz_Click;

            btnWyczysc = CreateStyledButton("Wyczyść", new Point(300, 110), Color.Orange);
            btnWyczysc.Click += BtnWyczysc_Click;

            btnEksport = CreateStyledButton("Eksportuj", new Point(420, 70), Color.LightBlue);
            btnEksport.Click += BtnEksport_Click;

            btnUsun = CreateStyledButton("Usuń", new Point(420, 110), Color.Red);
            btnUsun.Click += BtnUsun_Click;

            btnSearch = CreateStyledButton("Szukaj", new Point(540, 70), Color.Gray);
            btnSearch.Click += BtnSearch_Click;

            comboFilter = new ComboBox { Location = new Point(540, 110), Width = 120 };
            comboFilter.Items.AddRange(new string[] { "Wszystko", "Waga > 70", "Ciśnienie > 120/80", "Puls > 80" });
            comboFilter.SelectedIndex = 0;
            comboFilter.SelectedIndexChanged += ComboFilter_SelectedIndexChanged;

            // Таблица
            dataGridZdrowie = new DataGridView
            {
                Location = new Point(20, 200),
                Size = new Size(750, 300),
                ColumnCount = 4,
                BackgroundColor = Color.WhiteSmoke,
                GridColor = Color.Gray,
                DefaultCellStyle = new DataGridViewCellStyle { ForeColor = Color.Black, BackColor = Color.White },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.LightGray },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.DarkBlue }
            };
            dataGridZdrowie.Columns[0].Name = "Data";
            dataGridZdrowie.Columns[1].Name = "Waga";
            dataGridZdrowie.Columns[2].Name = "Ciśnienie";
            dataGridZdrowie.Columns[3].Name = "Puls";

            this.Controls.Add(labelWaga);
            this.Controls.Add(txtWaga);
            this.Controls.Add(labelCisnienie);
            this.Controls.Add(txtCisnienie);
            this.Controls.Add(labelPuls);
            this.Controls.Add(txtPuls);
            this.Controls.Add(btnZapisz);
            this.Controls.Add(btnWyczysc);
            this.Controls.Add(btnEksport);
            this.Controls.Add(btnUsun);
            this.Controls.Add(btnSearch);
            this.Controls.Add(comboFilter);
            this.Controls.Add(dataGridZdrowie);
        }

        private Button CreateStyledButton(string text, Point location, Color color)
        {
            return new Button
            {
                Text = text,
                Location = location,
                Width = 100,
                Height = 30,
                BackColor = color,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
        }

        private void LoadDataFromDatabase()
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=health.db;Version=3;"))
            {
                conn.Open();
                string sql = "SELECT Date, Weight, Pressure, Pulse FROM HealthData WHERE UserId = @UserId";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", UserId);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        dataGridZdrowie.Rows.Clear();

                        while (reader.Read())
                        {
                            dataGridZdrowie.Rows.Add(reader["Date"], reader["Weight"], reader["Pressure"], reader["Pulse"]);
                        }
                    }
                }
            }
        }

        private void BtnZapisz_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtWaga.Text) || string.IsNullOrWhiteSpace(txtCisnienie.Text) || string.IsNullOrWhiteSpace(txtPuls.Text))
            {
                MessageBox.Show("Wszystkie pola muszą być wypełnione!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=health.db;Version=3;"))
            {
                conn.Open();
                string sql = "INSERT INTO HealthData (UserId, Date, Weight, Pressure, Pulse) VALUES (@UserId, @Date, @Weight, @Pressure, @Pulse)";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", UserId);
                    cmd.Parameters.AddWithValue("@Date", DateTime.Now.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@Weight", txtWaga.Text);
                    cmd.Parameters.AddWithValue("@Pressure", txtCisnienie.Text);
                    cmd.Parameters.AddWithValue("@Pulse", txtPuls.Text);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Dane zostały zapisane!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDataFromDatabase();
                }
            }
        }

        private void BtnWyczysc_Click(object sender, EventArgs e)
        {
            txtWaga.Clear();
            txtCisnienie.Clear();
            txtPuls.Clear();
        }

        private void BtnEksport_Click(object sender, EventArgs e)
        {
            if (dataGridZdrowie.Rows.Count == 0)
            {
                MessageBox.Show("Brak danych do eksportu!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = "Zapisz dane jako CSV",
                FileName = "EksportowaneDane.csv"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                {
                    writer.WriteLine("Data,Waga,Ciśnienie,Puls");
                    foreach (DataGridViewRow row in dataGridZdrowie.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            writer.WriteLine($"{row.Cells[0].Value},{row.Cells[1].Value},{row.Cells[2].Value},{row.Cells[3].Value}");
                        }
                    }
                }
                MessageBox.Show("Dane zostały wyeksportowane!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnUsun_Click(object sender, EventArgs e)
        {
            if (dataGridZdrowie.SelectedRows.Count == 0)
            {
                MessageBox.Show("Wybierz rekord do usunięcia!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Czy na pewno chcesz usunąć wybrany rekord?", "Potwierdzenie", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=health.db;Version=3;"))
            {
                conn.Open();
                string sql = "DELETE FROM HealthData WHERE UserId = @UserId AND Date = @Date";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", UserId);
                    cmd.Parameters.AddWithValue("@Date", dataGridZdrowie.SelectedRows[0].Cells[0].Value.ToString());

                    int rowsDeleted = cmd.ExecuteNonQuery();

                    if (rowsDeleted > 0)
                    {
                        MessageBox.Show("Rekord został usunięty!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDataFromDatabase();
                    }
                    else
                    {
                        MessageBox.Show("Błąd podczas usuwania rekordu.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string input = Prompt.ShowDialog("Wpisz date w formacie YYYY-MM-DD:", "Wyszukiwanie");

            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Nie podano daty!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=health.db;Version=3;"))
            {
                conn.Open();
                string sql = "SELECT Date, Weight, Pressure, Pulse FROM HealthData WHERE UserId = @UserId AND Date = @Date";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", UserId);
                    cmd.Parameters.AddWithValue("@Date", input);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        dataGridZdrowie.Rows.Clear();
                        while (reader.Read())
                        {
                            dataGridZdrowie.Rows.Add(
                                reader["Date"].ToString(),
                                reader["Weight"].ToString(),
                                reader["Pressure"].ToString(),
                                reader["Pulse"].ToString()
                            );
                        }
                    }
                }
            }
        }

        private void ComboFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filter = comboFilter.SelectedItem.ToString();
            string sql = "SELECT Date, Weight, Pressure, Pulse FROM HealthData WHERE UserId = @UserId";

            if (filter == "Waga > 70")
            {
                sql += " AND Weight > 70";
            }
            else if (filter == "Ciśnienie > 120/80")
            {
                sql += " AND CAST(SUBSTR(Pressure, 1, INSTR(Pressure, '/')-1) AS INTEGER) > 120 " +
                       "AND CAST(SUBSTR(Pressure, INSTR(Pressure, '/')+1) AS INTEGER) > 80";
            }
            else if (filter == "Puls > 80")
            {
                sql += " AND Pulse > 80";
            }

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=health.db;Version=3;"))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", UserId);
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        dataGridZdrowie.Rows.Clear();
                        while (reader.Read())
                        {
                            dataGridZdrowie.Rows.Add(
                                reader["Date"].ToString(),
                                reader["Weight"].ToString(),
                                reader["Pressure"].ToString(),
                                reader["Pulse"].ToString()
                            );
                        }
                    }
                }
            }
        }

        // Funkция для вызова диалогового окна поиска
        public static class Prompt
        {
            public static string ShowDialog(string text, string caption)
            {
                Form prompt = new Form()
                {
                    Width = 400,
                    Height = 180,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    Text = caption,
                    StartPosition = FormStartPosition.CenterScreen
                };

                Label label = new Label() { Left = 20, Top = 20, Text = text, AutoSize = true };
                TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 340 };
                Button confirmation = new Button() { Text = "OK", Left = 270, Width = 90, Top = 80 };
                confirmation.DialogResult = DialogResult.OK;
                prompt.Controls.Add(label);
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.AcceptButton = confirmation;

                return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            }
        }
    }
}
