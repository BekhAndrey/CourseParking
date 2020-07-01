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
using КП.Models;
using КП.VIewModels;

namespace КП.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(User user)
        {
            InitializeComponent();
            Application.Current.MainWindow = Window.GetWindow(this);
            if(user.Username.ToLower() == "admin" && user.Password == "ISMvKXpXpadDiUoOSoAfww==")
            {
                MenuForAdmin.Visibility = Visibility.Visible;
            }
            DataContext = new MainViewModel(user);
            dp1.BlackoutDates.AddDatesInPast();
            dp1.BlackoutDates.Add(new CalendarDateRange(DateTime.Now, DateTime.Now.AddDays(2)));
            dp2.BlackoutDates.AddDatesInPast();
            dp3.BlackoutDates.AddDatesInPast();
            dp3.BlackoutDates.Add(new CalendarDateRange(DateTime.Now, DateTime.Now.AddDays(2)));
            dp4.BlackoutDates.AddDatesInPast();
        }

        private void dp1_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if(dp1.SelectedDate!=null)
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

        private void dp3_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dp3.SelectedDate != null)
            {
                DateTime date = (DateTime)dp3.SelectedDate;
                dp4.SelectedDate = date.AddDays(1);
                dp4.BlackoutDates.AddDatesInPast();
            }
            else
            {
                dp4.BlackoutDates.Clear();
                dp3.SelectedDate = DateTime.Now.AddDays(3);
                dp4.SelectedDate = DateTime.Now.AddDays(4);
                dp4.BlackoutDates.AddDatesInPast();
            }
        }

        private void dp4_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
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
