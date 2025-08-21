using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripExpenseNew.ViewModels
{
    public class HistoryItems : INotifyPropertyChanged
    {
        private string textTrip;
        private string iconLocationSource;
        private string textLocation;
        private string iconMileageSource;
        private string textMileage;
        private string iconDistanceSource;
        private string textDistance;
        public string TextTrip
        {
            get => textTrip;
            set
            {
                textTrip = value;
                OnPropertyChanged(nameof(TextTrip));
            }
        }
        public string IconLocationSource
        {
            get => iconLocationSource;
            set
            {
                iconLocationSource = value;
                OnPropertyChanged(nameof(IconLocationSource));
            }
        }
        public string TextLocation
        {
            get => textLocation;
            set
            {
                textLocation = value;
                OnPropertyChanged(nameof(TextLocation));
            }
        }

        public string IconMileageSource
        {
            get => iconMileageSource;
            set
            {
                iconMileageSource = value;
                OnPropertyChanged(nameof(IconMileageSource));
            }
        }

        public string TextMileage
        {
            get => textMileage;
            set
            {
                textMileage = value;
                OnPropertyChanged(nameof(TextMileage));
            }
        }

        public string IconDistanceSource
        {
            get => iconDistanceSource;
            set
            {
                iconDistanceSource = value;
                OnPropertyChanged(nameof(IconDistanceSource));
            }
        }

        public string TextDistance
        {
            get => textDistance;
            set
            {
                textDistance = value;
                OnPropertyChanged(nameof(TextDistance));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
