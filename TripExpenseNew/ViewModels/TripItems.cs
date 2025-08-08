using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripExpenseNew.ViewModels
{
    public class TripItems : INotifyPropertyChanged
    {
        private Color frameColor;
        private string iconLocationSource;
        private string textLocation;
        private string iconDateSource;
        private string textDate;
        private string textStatus;
        public Color FrameColor
        {
            get => frameColor;
            set
            {
                frameColor = value;
                OnPropertyChanged(nameof(FrameColor));
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

        public string IconDateSource
        {
            get => iconDateSource;
            set
            {
                iconDateSource = value;
                OnPropertyChanged(nameof(IconDateSource));
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

        public string TextDate
        {
            get => textDate;
            set
            {
                textDate = value;
                OnPropertyChanged(nameof(TextDate));
            }
        }

        public string TextStatus
        {
            get => textStatus;
            set
            {
                textStatus = value;
                OnPropertyChanged(nameof(TextStatus));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}