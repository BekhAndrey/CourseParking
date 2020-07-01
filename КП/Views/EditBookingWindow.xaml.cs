using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace КП.Views
{
    /// <summary>
    /// Логика взаимодействия для EditBookingWindow.xaml
    /// </summary>
    public partial class EditBookingWindow : Window
    {
        SqlConnection sqlCon = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=Parking;Integrated Security=True");
        ObservableCollection<ParkingPlace> collection;
        private DateTime TempEntry;
        private DateTime TempExit;
        private string TempNumber;
        private string TempVtype;
        private string TempPType;
        private int TempPrice;
        private int TempId;
        private ParkingPlace SelectedItem;
        public User U;
        public string Username
        {
            get { return U.Username; }
            set
            {
                SignedUser.Username = value;
            }
        }
        public double Price = 150;
        public bool Validation = true;
        public double mult;
        public User SignedUser;
        public EditBookingWindow(ParkingPlace selecteditem,User user)
        {
            InitializeComponent();
            U = user;
            DataContext = new EditViewModel(selecteditem, user);
        }
        private void SaveEdit_Execute(object sender, RoutedEventArgs e)
        {
            if (vtype.SelectedItem == null || ptype.SelectedItem == null || dp2.SelectedDate == null || Validation == false )
            {
                MessageBox.Show("Пожалуйста, заполните все поля и/или исправьте некорректные данные");
            }
            else
            {
                DialogResult = true;
                DateTime date1 = (DateTime)dp1.SelectedDate;
                DateTime date2 = (DateTime)dp2.SelectedDate;
                ParkingPlace newparking = new ParkingPlace();
                newparking.Entrydate = date1;
                newparking.Exitdate = date2;
                newparking.Carnumber = number.Text;
                newparking.Vehicletype = vtype.Text;
                newparking.Parkingtype = ptype.Text;
                TimeSpan span = date2 - date1;
                int days = span.Days;
                switch (vtype.Text)
                {
                    case "Автомобиль":
                        mult = 0.9;
                        break;

                    case "Мотоцикл":
                        mult = 0.8;
                        break;
                }
                switch (ptype.Text)
                {
                    case "Крытая":
                        mult = mult + 0.5;
                        break;

                    case "Открытая":
                        mult = mult + 0.1;
                        break;
                }
                Price = (days * 150) * mult;
                try
                {
                    sqlCon.Open();
                    string newquery = "SELECT COUNT(*) FROM ParkingPlace WHERE ParkingType = @ptype AND @entrydate BETWEEN EnterDate AND ExitDate";
                    SqlCommand newcmd = new SqlCommand(newquery, sqlCon);
                    newcmd.CommandType = CommandType.Text;
                    newcmd.Parameters.AddWithValue("@ptype", ptype.Text);
                    newcmd.Parameters.AddWithValue("@entrydate", date1);
                    int count = Convert.ToInt32(newcmd.ExecuteScalar());
                    if (count < 20)
                    {
                        MessageBoxResult result = MessageBox.Show($"Итоговая цена: {Convert.ToInt32(Price).ToString()} \nПродолжить?", "Подтверждение бронирования", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                        if (result == MessageBoxResult.Yes)
                        {
                            collection.Remove(SelectedItem);
                            collection.Add(newparking);
                            string query = "UPDATE ParkingPlace SET EnterDate=@enterdate, ExitDate=@exitdate, CarNumber=@carnumber, VehicleType=@vehicletype, Price = @price, ParkingType=@parkingtype WHERE Username=@username AND Id=@id";
                            SqlCommand sqlcmd = new SqlCommand(query, sqlCon);
                            sqlcmd.CommandType = CommandType.Text;
                            sqlcmd.Parameters.AddWithValue("@enterdate", newparking.Entrydate);
                            sqlcmd.Parameters.AddWithValue("@exitdate", newparking.Exitdate);
                            sqlcmd.Parameters.AddWithValue("@carnumber", newparking.Carnumber);
                            sqlcmd.Parameters.AddWithValue("@vehicletype", newparking.Vehicletype);
                            sqlcmd.Parameters.AddWithValue("@parkingtype", newparking.Parkingtype);
                            sqlcmd.Parameters.AddWithValue("@id", TempId);
                            sqlcmd.Parameters.AddWithValue("@price", Price);
                            sqlcmd.Parameters.AddWithValue("@username", Username);
                            sqlcmd.ExecuteNonQuery();
                            MainWindow wnd = new MainWindow(U);
                            wnd.Show();
                            foreach (Window item in App.Current.Windows)
                            {
                                if (item != wnd)
                                    item.Close();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("К сожалению, свободных мест для выбранного типа парковки нет");
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
        }

        private void dp1_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dp1.SelectedDate != null)
            {
                DateTime date = (DateTime)dp1.SelectedDate;
                dp2.SelectedDate = date.AddDays(1);
                dp2.BlackoutDates.Add(new CalendarDateRange(DateTime.Now, date));
            }
            else
            {
                dp2.BlackoutDates.Clear();
                dp1.SelectedDate = DateTime.Now.AddDays(3);
                dp2.SelectedDate = DateTime.Now.AddDays(4);
                dp2.BlackoutDates.AddDatesInPast();
            }
        }

        private void dp2_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dp2.SelectedDate == null)
            {
                dp2.BlackoutDates.Clear();
                DateTime date = (DateTime)dp1.SelectedDate;
                dp2.SelectedDate = date.AddDays(1);
                dp2.BlackoutDates.AddDatesInPast();
            }
        }
    }
}
