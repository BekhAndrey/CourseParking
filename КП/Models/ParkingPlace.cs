using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace КП.Models
{
    public class ParkingPlace : INotifyPropertyChanged
    {
        private DateTime entrydate;
        private DateTime exitdate;
        private string carnumber;
        private string vehicletype;
        private string parkingtype;
        private int price;
        private string username;
        private string status;
        private int id;

        public int Id
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }

        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                OnPropertyChanged("Username");
            }
        }

        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged("Status");
            }
        }

        public int Price
        {
            get { return price; }
            set
            {
                price = value;
                OnPropertyChanged("Price");
            }
        }


        public DateTime Entrydate
        {
            get { return entrydate; }
            set
            {
                entrydate = value;
                OnPropertyChanged("Entrydate");
            }
        }

        public DateTime Exitdate
        {
            get { return exitdate; }
            set
            {
                exitdate = value;
                OnPropertyChanged("Exitdate");
            }
        }
        public string Carnumber
        {
            get { return carnumber; }
            set
            {
                carnumber = value;
                OnPropertyChanged("Carnumber");
            }
        }
        public string Vehicletype
        {
            get { return vehicletype; }
            set
            {
                vehicletype = value;
                OnPropertyChanged("Vehicletype");
            }
        }

        public string Parkingtype
        {
            get { return parkingtype; }
            set
            {
                parkingtype = value;
                OnPropertyChanged("Parkingtype");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
