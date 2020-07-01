using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using КП.Views;

namespace КП.VIewModels
{
    class ForgotViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public bool Validation = true;
        public Dictionary<string, string> ErrorCollection { get; private set; } = new Dictionary<string, string>();
        public string this[string columnName]
        {
            get
            {
                string error = String.Empty;
                switch (columnName)
                {
                    case "Email":
                        Regex regex = new Regex(@"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$");
                        MatchCollection matches = regex.Matches(Email);
                        if (matches.Count <= 0)
                        {
                            error = "Некорректный email";
                            Validation = false;
                        }
                        else
                        {
                            Validation = true;
                        }
                        break;
                }
                if (ErrorCollection.ContainsKey(columnName))
                    ErrorCollection[columnName] = error;
                else if (error != null)
                    ErrorCollection.Add(columnName, error);
                OnPropertyChanged("ErrorCollection");
                return error;
            }
        }
        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        private string username;
        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                OnPropertyChanged("Email");
            }
        }

        private string email="";
        public string Email
        {
            get { return email; }
            set
            {
                email = value;
                OnPropertyChanged("Email");
            }
        }

        private string code;
        public string Code
        {
            get { return code; }
            set
            {
                code = value;
                OnPropertyChanged("Code");
            }
        }

        public string ConfirmCode;

        private RelayCommand send;
        public RelayCommand Send
        {
            get
            {
                return send ??
                    (send = new RelayCommand(obj =>
                    {
                        if(Validation)
                        {
                            SqlConnection sqlCon1 = null;
                            try
                            {
                                sqlCon1 = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=Parking;Integrated Security=True");
                                if (sqlCon1.State == ConnectionState.Closed)
                                    sqlCon1.Open();
                                string query = "SELECT COUNT(1) FROM tblUser WHERE Username=@username AND Email=@email";
                                SqlCommand sqlcmd = new SqlCommand(query, sqlCon1);
                                sqlcmd.CommandType = CommandType.Text;
                                sqlcmd.Parameters.AddWithValue("@username", Username);
                                sqlcmd.Parameters.AddWithValue("@email", Email);
                                int count = Convert.ToInt32(sqlcmd.ExecuteScalar());
                                if (count == 1)
                                {
                                    Random rnd = new Random();
                                    int value = rnd.Next(10000, 99999);
                                    ConfirmCode = value.ToString();
                                    MailAddress from = new MailAddress("andreyac17@gmail.com", "Автостоянка");
                                    MailAddress to = new MailAddress(Email);
                                    MailMessage m = new MailMessage(from, to);
                                    m.Subject = "Восстановление пароля";
                                    m.Body = $"Код для восстановления пароля: {ConfirmCode}";
                                    SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                                    smtp.Credentials = new NetworkCredential("andreyac17@gmail.com", "Ghbdtn3Vbh4");
                                    smtp.EnableSsl = true;
                                    smtp.Send(m);
                                }
                                else
                                {
                                    MessageBox.Show("Адрес электронной почты или имя пользователя не существует");
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                            finally
                            {
                                if (sqlCon1 != null)
                                    sqlCon1.Close();
                            }
                        }
                    }));
            }
        }
        private RelayCommand confirm;
        public RelayCommand Confirm
        {
            get
            {
                return confirm ??
                    (confirm = new RelayCommand(obj =>
                    {
                      if(ConfirmCode==Code)
                        {
                            ChangePass pass = new ChangePass(Username);
                            pass.Show();
                        }
                        else
                        {
                            MessageBox.Show("Неправильный код");
                        }
                    }));
            }
        }
    }
}
