using DocumentFormat.OpenXml.Bibliography;
using System.ComponentModel.DataAnnotations;
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
using System.Windows.Controls;
using КП.Models;
using КП.Views;
using Microsoft.Build.Framework;

namespace КП.VIewModels
{
    class MainViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        SqlConnection sqlCon = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=Parking;Integrated Security=True");
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public bool Validation = true;
        public Dictionary<string,string> ErrorCollection { get; private set; } = new Dictionary<string, string>();
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
        public string Username
        {
            get { return SignedUser.Username; }
            set 
            {
                SignedUser.Username = value;
                OnPropertyChanged("Username");
            }
        }
        public string Password
        {
            get { return SignedUser.Password; }
            set
            {
                SignedUser.Password = value;
                OnPropertyChanged("Password");
            }
        }
        public string Email
        {
            get { return SignedUser.Email; }
            set
            {
                SignedUser.Email = value;
                OnPropertyChanged("Email");
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
        public double mult;
        public User SignedUser;
        public MainViewModel(User user)
        {
            this.SignedUser = user;
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

            try
            {
                string sql = "SELECT EnterDate, ExitDate, CarNumber, VehicleType, ParkingType, Price, Id FROM ParkingPlace WHERE Username=@username AND DATEDIFF(dd, GETDATE(), EnterDate)>2";
                SqlCommand command = new SqlCommand(sql, sqlCon);
                command.Parameters.AddWithValue("@username", Username);
                sqlCon.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ParkingPlace place = new ParkingPlace();
                    place.Entrydate = reader.GetDateTime(0);
                    place.Exitdate = reader.GetDateTime(1);
                    place.Carnumber = reader.GetString(2);
                    place.Vehicletype = reader.GetString(3);
                    place.Parkingtype = reader.GetString(4);
                    place.Price = reader.GetInt32(5);
                    place.Id = reader.GetInt32(6);
                    ParkingCollection.Add(place);
                }
                reader.Close();

                string mail = "SELECT Email, Username FROM tblUser where Username = @username";
                SqlCommand newcmd = new SqlCommand(mail, sqlCon);
                newcmd.Parameters.AddWithValue("@username", Username);
                SqlDataReader newreader = newcmd.ExecuteReader();
                while (newreader.Read())
                {
                    Email = newreader.GetString(0);
                    Username = newreader.GetString(1);
                }
                newreader.Close();

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

        public void UpdateP()
        {
            try
            {
                sqlCon.Open();
                AdminCollection.Clear();
                string statusr = "UPDATE ParkingPlace SET Status = @statusr WHERE GETDATE() BETWEEN EnterDate AND ExitDate";
                SqlCommand running = new SqlCommand(statusr, sqlCon);
                running.CommandType = CommandType.Text;
                running.Parameters.AddWithValue("@statusr", "Заезд подтвержден");
                running.ExecuteNonQuery();
                string statusd = "UPDATE ParkingPlace SET Status = @statusd WHERE ExitDate < GETDATE()";
                SqlCommand done = new SqlCommand(statusd, sqlCon);
                done.CommandType = CommandType.Text;
                done.Parameters.AddWithValue("@statusd", "Бронирование завершено");
                done.ExecuteNonQuery();
                string sql = "SELECT EnterDate, ExitDate, CarNumber, VehicleType, ParkingType, Price, Username, Status, Id FROM ParkingPlace ORDER BY Status";
                SqlCommand command = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ParkingPlace place = new ParkingPlace();
                    place.Entrydate = reader.GetDateTime(0);
                    place.Exitdate = reader.GetDateTime(1);
                    place.Carnumber = reader.GetString(2);
                    place.Vehicletype = reader.GetString(3);
                    place.Parkingtype = reader.GetString(4);
                    place.Price = reader.GetInt32(5);
                    place.Username = reader.GetString(6);
                    place.Status = reader.GetString(7);
                    place.Id = reader.GetInt32(8);
                    AdminCollection.Add(place);
                }
                reader.Close();
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

        public void UpdateU()
        {
            try
            {
                UserCollection.Clear();
                string sql = "SELECT Username, Password, Email FROM tblUser WHERE Username != @admin";
                SqlCommand command = new SqlCommand(sql, sqlCon);
                command.Parameters.AddWithValue("@admin", "admin");
                sqlCon.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    User user = new User();
                    user.Username = reader.GetString(0);
                    user.Password = reader.GetString(1);
                    user.Email = reader.GetString(2);
                    UserCollection.Add(user);
                }
                reader.Close();
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

        private RelayCommand updatePCol;
        public RelayCommand UpdatePCol
        {
            get
            {
                return updatePCol ??
                    (updatePCol = new RelayCommand(obj =>
                    {
                        UpdateP();
                    }));
            }
        }

        private RelayCommand updateUCol;
        public RelayCommand UpdateUCol
        {
            get
            {
                return updateUCol ??
                    (updateUCol = new RelayCommand(obj =>
                    {
                        UpdateU();
                    }));
            }
        }

        public string currentUsername;
        public string CurrentUsername
        {
            get { return currentUsername; }
            set
            {
                currentUsername = value;
                OnPropertyChanged("CurrentUsername");
            }
        }
        public string currentPassword;
        public string CurrentPassword
        {
            get { return currentPassword; }
            set
            {
                currentPassword = value;
                OnPropertyChanged("CurrentPassword");
            }
        }
        public string currentEmail;
        public string CurrentEmail
        {
            get { return currentEmail; }
            set
            {
                currentEmail = value;
                OnPropertyChanged("CurrentEmail");
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

        private ObservableCollection<ParkingPlace> _parkingCollection;
        public ObservableCollection<ParkingPlace> ParkingCollection
        {
            get { return _parkingCollection ?? (_parkingCollection = new ObservableCollection<ParkingPlace>()); }
            set
            {
                _parkingCollection = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ParkingPlace> _admincollection;
        public ObservableCollection<ParkingPlace> AdminCollection
        {
            get { return _admincollection ?? (_admincollection = new ObservableCollection<ParkingPlace>()); }
            set
            {
                _admincollection = value;
                OnPropertyChanged();
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
        private ObservableCollection<User> _usercollection;
        public ObservableCollection<User> UserCollection
        {
            get { return _usercollection ?? (_usercollection = new ObservableCollection<User>()); }
            set
            {
                _usercollection = value;
                OnPropertyChanged();
            }
        }

        private User selectedUser;
        public User SelectedUser
        {
            get { return selectedUser; }
            set
            {
                selectedUser = value;
                OnPropertyChanged("SelectedUser");
            }
        }

        public string carNumber= "";
        public string CarNumber
        {
            get { return carNumber; }
            set
            {
                carNumber = value;
                OnPropertyChanged("CarNumber");
            }
        }

        private ObservableCollection<vtype> _vtypes;

        public ObservableCollection<vtype> VType
        {
            get { return _vtypes; }
            set { _vtypes = value; }
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


        private ObservableCollection<ptype> _ptypes;

        public ObservableCollection<ptype> PType
        {
            get { return _ptypes; }
            set { _ptypes = value; }
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

        private RelayCommand logout;
        public RelayCommand Logout
        {
            get
            {
                return logout ??
                    (logout = new RelayCommand(obj =>
                    {
                        LoginWindow wnd = new LoginWindow();
                        wnd.Show();
                        foreach (Window item in App.Current.Windows)
                        {
                            if (item != wnd)
                                item.Close();
                        }
                    }));
            }
        }
        private RelayCommand changePass;
        public RelayCommand ChangePass
        {
            get
            {
                return changePass ??
                    (changePass = new RelayCommand(obj =>
                    {
                        EditPassword edit = new EditPassword(Password, Username);
                        edit.Show();
                    }));
            }
        }
        private RelayCommand newBooking;
        public RelayCommand NewBooking
        {
            get
            {
                return newBooking ??
                    (newBooking = new RelayCommand(obj =>
                    {
                    try
                    {
                            sqlCon.Open();
                            if (Validation && !String.IsNullOrEmpty(CarNumber) && !String.IsNullOrEmpty(Exit.ToString()) && vehtype!=null && parkingtype != null)
                            {
                                TimeSpan span = Exit - Entry;
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
                                Price = (days * 150)*mult;
                           
                                string newquery = "SELECT COUNT(*) FROM ParkingPlace WHERE ParkingType = @ptype AND @entrydate BETWEEN EnterDate AND ExitDate";
                                SqlCommand newcmd = new SqlCommand(newquery, sqlCon);
                                newcmd.CommandType = CommandType.Text;
                                newcmd.Parameters.AddWithValue("@ptype", parkingtype.ToString());
                                newcmd.Parameters.AddWithValue("@entrydate", Entry);
                                int count = Convert.ToInt32(newcmd.ExecuteScalar());
                                if (count < 20)
                                {
                                    MessageBoxResult result = MessageBox.Show($"Итоговая цена: {Convert.ToInt32(Price).ToString()} \nПродолжить?", "Подтверждение бронирования", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                                    if (result == MessageBoxResult.Yes)
                                    {
                                        string query = "INSERT INTO ParkingPlace(EnterDate, ExitDate, CarNumber,VehicleType,ParkingType,Username, Price, Status) VALUES(@enterdate, @exitdate, @carnumber, @vehicletype, @parkingtype, @username, @price, @status)";
                                        SqlCommand sqlcmd = new SqlCommand(query, sqlCon);
                                        sqlcmd.CommandType = CommandType.Text;
                                        sqlcmd.Parameters.AddWithValue("@enterdate", Entry);
                                        sqlcmd.Parameters.AddWithValue("@exitdate", Exit);
                                        sqlcmd.Parameters.AddWithValue("@carnumber", CarNumber);
                                        sqlcmd.Parameters.AddWithValue("@vehicletype", vehtype.ToString());
                                        sqlcmd.Parameters.AddWithValue("@parkingtype", parkingtype.ToString());
                                        sqlcmd.Parameters.AddWithValue("@username", Username);
                                        sqlcmd.Parameters.AddWithValue("@price", Price);
                                        sqlcmd.Parameters.AddWithValue("@status", "Ожидается заезд");
                                        sqlcmd.ExecuteNonQuery();
                                        ParkingCollection.Add(new ParkingPlace() { Entrydate = Entry, Exitdate = Exit, Carnumber = CarNumber, Vehicletype = vehtype.ToString(), Parkingtype = parkingtype.ToString(), Price = Convert.ToInt32(Price), Status = "Ожидается заезд" });
                                        MainWindow wnd = new MainWindow(SignedUser);
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
                            else
                            {
                                MessageBox.Show("Пожалуйста, заполните все поля и/или исправьте некорректные данные");
                            }
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            if (sqlCon != null)
                                sqlCon.Close();
                        }
                    }));
            }
        }

        private RelayCommand delBooking;
        public RelayCommand DelBooking
        {
            get
            {
                return delBooking ??
                    (delBooking = new RelayCommand(obj =>
                    {
                        try
                        {
                            if (SelectedPlace != null)
                            {
                                MessageBoxResult del = MessageBox.Show("Отменить выбранное бронирование?", "Отменить", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                                if (del == MessageBoxResult.Yes)
                                {
                                    string query = "DELETE FROM ParkingPlace WHERE Username=@username AND Id=@id ";
                                    SqlCommand sqlcmd = new SqlCommand(query, sqlCon);
                                    sqlcmd.CommandType = CommandType.Text;
                                    sqlcmd.Parameters.AddWithValue("@id", SelectedPlace.Id);
                                    sqlcmd.Parameters.AddWithValue("@username", Username);
                                    sqlCon.Open();
                                    sqlcmd.ExecuteNonQuery();
                                    ParkingCollection.Remove(SelectedPlace);
                                    selectedPlace = null;
                                }
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
                    }));
            }
        }
        private RelayCommand delAdmin;
        public RelayCommand DelAdmin
        {
            get
            {
                return delAdmin ??
                    (delAdmin = new RelayCommand(obj =>
                    {
                        try
                        {
                            if (SelectedPlace != null)
                            {
                                MessageBoxResult del = MessageBox.Show("Удалить запись", "Удалить", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                                if (del == MessageBoxResult.Yes)
                                {
                                    string query = "DELETE FROM ParkingPlace WHERE Id=@id ";
                                    SqlCommand sqlcmd = new SqlCommand(query, sqlCon);
                                    sqlcmd.CommandType = CommandType.Text;
                                    sqlcmd.Parameters.AddWithValue("@id", SelectedPlace.Id);
                                    sqlCon.Open();
                                    sqlcmd.ExecuteNonQuery();
                                    AdminCollection.Remove(SelectedPlace);
                                    selectedPlace = null;
                                }
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
                    }));
            }
        }
        private RelayCommand editBooking;
        public RelayCommand EditBooking
        {
            get
            {
                return editBooking ??
                    (editBooking = new RelayCommand(obj =>
                    {
                        if (SelectedPlace != null)
                        {
                            EditBookingWindow editWindow = new EditBookingWindow(SelectedPlace, SignedUser);
                            editWindow.Show();
                        }
                    }));
            }
        }
    }
}
