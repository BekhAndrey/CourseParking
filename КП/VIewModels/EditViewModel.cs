using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using КП.Models;
using КП.Views;

namespace КП.VIewModels
{
    class EditViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        SqlConnection sqlCon = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=Parking;Integrated Security=True");
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public bool Validation = false;
        public Dictionary<string, string> ErrorCollection { get; private set; } = new Dictionary<string, string>();
        public string this[string columnName]
        {
            get
            {
                string error = String.Empty;
                switch (columnName)
                {
                    case "CarNumber":
                        Regex regex = new Regex(@"^[A-Za-z0-9]*$");
                        MatchCollection matches = regex.Matches(CarNumber);
                        if (matches.Count <= 0)
                        {
                            error = "Номер не должен содержать нелатинские символы";
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

        private ObservableCollection<vtype> _vtypes;

        public ObservableCollection<vtype> VType
        {
            get { return _vtypes; }
            set { _vtypes = value; }
        }

        private ObservableCollection<ptype> _ptypes;

        public ObservableCollection<ptype> PType
        {
            get { return _ptypes; }
            set { _ptypes = value; }
        }

        public EditViewModel(ParkingPlace place, User user)
        {
            VType = new ObservableCollection<vtype>()
            {
                new vtype(){Title="Автомобиль"},
                new vtype(){Title="Мотоцикл"},
            };
            PType = new ObservableCollection<ptype>()
            {
                new ptype(){Title="Крытая"},
                new ptype(){Title="Открытая"},
            };
            U = user;
            SelectedPlace = place;
            Entry = SelectedPlace.Entrydate;
            Exit = SelectedPlace.Exitdate;
            CarNumber = SelectedPlace.Carnumber;
            Id = SelectedPlace.Id;
        }

        public User U;

        public string Username
        {
            get { return U.Username; }
            set
            {
                U.Username = value;
                OnPropertyChanged("Username");
            }
        }

        public double id;
        public double Id
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }

        public double price;
        public double Price
        {
            get { return price; }
            set
            {
                price = value;
                OnPropertyChanged("Price");
            }
        }

        public DateTime entry = DateTime.Now.AddDays(3);
        public DateTime Entry
        {
            get { return entry; }
            set
            {
                entry = value;
                OnPropertyChanged("Entry");
            }
        }

        public DateTime exit = DateTime.Now.AddDays(4);
        public DateTime Exit
        {
            get { return exit; }
            set
            {
                exit = value;
                OnPropertyChanged("Exit");
            }
        }

        private ParkingPlace selectedPlace;
        public ParkingPlace SelectedPlace
        {
            get { return selectedPlace; }
            set
            {
                selectedPlace = value;
                OnPropertyChanged("SelectedPlace");
            }
        }

        public string carNumber = "";
        public string CarNumber
        {
            get { return carNumber; }
            set
            {
                carNumber = value;
                OnPropertyChanged("CarNumber");
            }
        }

        private vtype _vtype;

        public vtype vehtype
        {
            get { return _vtype; }
            set
            {
                _vtype = value;
                OnPropertyChanged("vehtype");
            }
        }

        private ptype _ptype;

        public ptype parkingtype
        {
            get { return _ptype; }
            set
            {
                _ptype = value;
                OnPropertyChanged("parkingtype");
            }
        }

        public double mult;
        private RelayCommand editBooking;
        public RelayCommand EditBooking
        {
            get
            {
                return editBooking ??
                    (editBooking = new RelayCommand(obj =>
                    {
                        if (vehtype== null || parkingtype == null || Exit == null || Entry==null || Validation == false)
                        {
                            MessageBox.Show("Пожалуйста, заполните все поля и/или исправьте некорректные данные");
                        }
                        else
                        {
                            DateTime date1 = (DateTime)Entry;
                            DateTime date2 = (DateTime)Exit;
                            TimeSpan span = date2 - date1;
                            int days = span.Days;
                            switch (vehtype.ToString())
                            {
                                case "Автомобиль":
                                    mult = 0.9;
                                    break;

                                case "Мотоцикл":
                                    mult = 0.8;
                                    break;
                            }
                            switch (parkingtype.ToString())
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
                                newcmd.Parameters.AddWithValue("@ptype", parkingtype.ToString());
                                newcmd.Parameters.AddWithValue("@entrydate", date1);
                                int count = Convert.ToInt32(newcmd.ExecuteScalar());
                                if (count < 20)
                                {
                                    MessageBoxResult result = MessageBox.Show($"Итоговая цена: {Convert.ToInt32(Price).ToString()} \nПродолжить?", "Подтверждение бронирования", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                                    if (result == MessageBoxResult.Yes)
                                    {
                                        string query = "UPDATE ParkingPlace SET EnterDate=@enterdate, ExitDate=@exitdate, CarNumber=@carnumber, VehicleType=@vehicletype, Price = @price, ParkingType=@parkingtype WHERE Username=@username AND Id=@id";
                                        SqlCommand sqlcmd = new SqlCommand(query, sqlCon);
                                        sqlcmd.CommandType = CommandType.Text;
                                        sqlcmd.Parameters.AddWithValue("@enterdate", Entry);
                                        sqlcmd.Parameters.AddWithValue("@exitdate", Exit);
                                        sqlcmd.Parameters.AddWithValue("@carnumber", CarNumber);
                                        sqlcmd.Parameters.AddWithValue("@vehicletype", vehtype.ToString());
                                        sqlcmd.Parameters.AddWithValue("@parkingtype", parkingtype.ToString());
                                        sqlcmd.Parameters.AddWithValue("@id", Id);
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
                    }));
            }
        }
    }
}
