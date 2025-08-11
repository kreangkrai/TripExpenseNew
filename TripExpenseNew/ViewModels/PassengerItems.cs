using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripExpenseNew.ViewModels
{
    public class PassengerItems : INotifyPropertyChanged
    {
        private string iconDatePassengerSource;
        private string textDatePassenger;
        private string textPassenger;

        public string IconDatePassengerSource
        {
            get => iconDatePassengerSource;
            set
            {
                iconDatePassengerSource = value;
                OnPropertyChanged(nameof(IconDatePassengerSource));
            }
        }

        public string TextDatePassenger
        {
            get => textDatePassenger;
            set
            {
                textDatePassenger = value;
                OnPropertyChanged(nameof(TextDatePassenger));
            }
        }

        public string TextPassenger
        {
            get => textPassenger;
            set
            {
                textPassenger = value;
                OnPropertyChanged(nameof(TextPassenger));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}