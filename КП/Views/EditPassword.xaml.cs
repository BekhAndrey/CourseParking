using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using КП.VIewModels;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace КП.Views
{
    public partial class EditPassword : Window
    {
        public string Pass;
        public string Username;
        public bool pvalid = false;
        SqlConnection sqlCon = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=Parking;Integrated Security=True");
        public EditPassword(string password, string username)
        {
            Username = username;
            Pass = password;
            InitializeComponent();
        }

        public string GetHash(string input)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            return Convert.ToBase64String(hash);
        }

        private void Change(object sender, ExecutedRoutedEventArgs e)
        {
            string oldpass = GetHash(old.Password);
            if(pvalid)
            {
                if (Pass == oldpass)
                {
                    try
                    {
                        sqlCon.Open();
                        string query = "UPDATE tblUser SET Password=@password WHERE Username=@username";
                        SqlCommand sqlcmd = new SqlCommand(query, sqlCon);
                        sqlcmd.CommandType = CommandType.Text;
                        sqlcmd.Parameters.AddWithValue("@password", newpass.Password);
                        sqlcmd.Parameters.AddWithValue("@username", Username);
                        sqlcmd.ExecuteNonQuery();
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        if (sqlCon != null)
                            sqlCon.Close();
                    }
                }
                else
                {
                    old.BorderBrush = Brushes.Red;
                    old.ToolTip = "Старый пароль не совпадает";
                }
            }
        }

        private void Pass_LostFocus(object sender, RoutedEventArgs e)
        {
                string pattern = @"\w{6,}";
                if (!Regex.IsMatch(newpass.Password, pattern, RegexOptions.IgnoreCase))
                {
                    pvalid = false;
                    newpass.BorderBrush = Brushes.Red;
                    newpass.ToolTip = "Пароль слишком короткий";
                }
                else
                {
                    pvalid = true;
                    newpass.ToolTip = null;
                    newpass.BorderBrush = Brushes.White;
                }
        }
    }
}
