using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using КП.Models;
using КП.VIewModels;
using System.Security.Cryptography;

namespace КП.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }


        public string GetHash(string input)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            return Convert.ToBase64String(hash);
        }

        public bool evalid = false;
        public bool uvalid = false;
        public bool pvalid = false;
        private void Login(object sender, ExecutedRoutedEventArgs e)
        {
            SqlConnection sqlCon1 = null;
            try
            {
                sqlCon1 = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=Parking;Integrated Security=True");
                sqlCon1.Open();
                string query = "SELECT COUNT(1) FROM tblUser WHERE Username=@username AND Password=@password";
                SqlCommand sqlcmd = new SqlCommand(query, sqlCon1);
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.Parameters.AddWithValue("@username", UsernameBox.Text);
                sqlcmd.Parameters.AddWithValue("@password", PasswordBox.Password);
                int count = Convert.ToInt32(sqlcmd.ExecuteScalar());
                if (count == 1)
                {
                    string pass = GetHash(PasswordBox.Password);
                    User user = new User();
                    user.Username = UsernameBox.Text;
                    user.Password = pass;
                    MainWindow wnd = new MainWindow(user);
                    wnd.Show();
                    this.Close();
                }
                else
                {
                    Error.Visibility=Visibility.Visible;
                }
        }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if(sqlCon1!=null) 
                    sqlCon1.Close();
            }
        }

        private void Register(object sender, ExecutedRoutedEventArgs e)
        {
            SqlConnection sqlCon2 = null;
            try
            {
                sqlCon2 = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=Parking;Integrated Security=True");
                if(evalid && pvalid && uvalid)
                {
                    if (RPassword.Password == ConfirmPassword.Password)
                    {
                        if (sqlCon2.State == ConnectionState.Closed)
                            sqlCon2.Open();
                        string query = "INSERT INTO tblUser (Username, Password, Email) VALUES (@username, @password, @email)";
                        SqlCommand sqlcmd = new SqlCommand(query, sqlCon2);
                        sqlcmd.CommandType = CommandType.Text;
                        sqlcmd.Parameters.AddWithValue("@username", RUsername.Text);
                        sqlcmd.Parameters.AddWithValue("@password", RPassword.Password);
                        sqlcmd.Parameters.AddWithValue("@email", Email.Text);
                        sqlcmd.ExecuteNonQuery();
                        User user = new User();
                        user.Username = RUsername.Text;
                        user.Password = RPassword.Password;
                        user.Email = Email.Text;
                        MainWindow wnd = new MainWindow(user);
                        wnd.Show();
                    }
                    else
                    {
                        ConfirmPassword.BorderBrush = Brushes.Red;
                        ConfirmPassword.ToolTip = "Пароли не совпадают";
                    }
                }
            }
            catch (SqlException ex)
            {
                switch (ex.Number)
                {
                    case 2627:
                        RUsername.BorderBrush = Brushes.Red;
                        Email.BorderBrush = Brushes.Red;
                        regErr.Visibility = Visibility.Visible;
                        break;
                    default:
                        throw;
                }
            }
            finally
            {
                if (sqlCon2!= null)
                    sqlCon2.Close();
            }
        }

        private void Email_LostFocus(object sender, RoutedEventArgs e)
        {
            string pattern = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$";
            if (!Regex.IsMatch(Email.Text, pattern, RegexOptions.IgnoreCase))
            {
                evalid = false;
                Email.BorderBrush = Brushes.Red;
                Email.ToolTip = "Некорректный email";
            }
            else
            {
                evalid = true;
                Email.ToolTip = null;
                Email.BorderBrush = Brushes.White;
            }
        }

        private void RPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            string pattern = @"\w{6,}";
            if (!Regex.IsMatch(RPassword.Password, pattern, RegexOptions.IgnoreCase))
            {
                pvalid = false;
                RPassword.BorderBrush = Brushes.Red;
                RPassword.ToolTip = "Пароль слишком короткий";
            }
            else
            {
                pvalid = true;
                RPassword.ToolTip = null;
                RPassword.BorderBrush = Brushes.White;
            }
        }

        private void RestorePass(object sender, ExecutedRoutedEventArgs e)
        {
            ForgotPassword restore = new ForgotPassword();
            restore.Show();
        }

        private void U_LostFocus(object sender, RoutedEventArgs e)
        {
            string pattern = @"^[a-zA-Z][a-zA-Z0-9-_\.]{1,20}$";
            if (!Regex.IsMatch(RUsername.Text, pattern, RegexOptions.IgnoreCase))
            {
                uvalid = false;
                RUsername.BorderBrush = Brushes.Red;
                RUsername.ToolTip = "Имя пользователя должно начинаться с буквы и не должно содержать специальных символов";
            }
            else
            {
                uvalid = true;
                RUsername.ToolTip = null;
                RUsername.BorderBrush = Brushes.White;
            }
        }
    }
}
