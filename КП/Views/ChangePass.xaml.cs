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

namespace КП.Views
{
    /// <summary>
    /// Логика взаимодействия для ChangePass.xaml
    /// </summary>
    public partial class ChangePass : Window
    {
        SqlConnection sqlCon = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=Parking;Integrated Security=True");
        public string Username;
        public bool pvalid = true;
        public ChangePass(string username)
        {
            Username = username;
            InitializeComponent();
        }

        private void Change(object sender, ExecutedRoutedEventArgs e)
        {
            if(pvalid)
            {
                if (pass.Password == confirm.Password)
                {
                    try
                    {
                        sqlCon.Open();
                        string query = "UPDATE tblUser SET Password=@password WHERE Username=@username";
                        SqlCommand sqlcmd = new SqlCommand(query, sqlCon);
                        sqlcmd.CommandType = CommandType.Text;
                        sqlcmd.Parameters.AddWithValue("@password", pass.Password);
                        sqlcmd.Parameters.AddWithValue("@username", Username);
                        sqlcmd.ExecuteNonQuery();
                        LoginWindow wnd = new LoginWindow();
                        wnd.Show();
                        foreach (Window item in App.Current.Windows)
                        {
                            if (item != wnd)
                                item.Close();
                        }
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
                    confirm.BorderBrush = Brushes.Red;
                    confirm.ToolTip = "Пароли не совпадает";
                }
            }
        }

        private void Pass_LostFocus(object sender, RoutedEventArgs e)
        {
            string pattern = @"\w{6,}";
            if (!Regex.IsMatch(pass.Password, pattern, RegexOptions.IgnoreCase))
            {
                pvalid = false;
                pass.BorderBrush = Brushes.Red;
                pass.ToolTip = "Пароль слишком короткий";
            }
            else
            {
                pvalid = true;
                pass.ToolTip = null;
                pass.BorderBrush = Brushes.White;
            }
        }
    }
}
