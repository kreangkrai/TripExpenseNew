using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TripExpenseNew.Interface;
using TripExpenseNew.Models;
using TripExpenseNew.Services;

namespace TripExpenseNew.ViewModels
{
    public class EmployeeItems : INotifyPropertyChanged
    {
        private ObservableCollection<EmployeeModel> _employees = new ObservableCollection<EmployeeModel> ();
        private ObservableCollection<EmployeeModel> _filteredEmployees = new ObservableCollection<EmployeeModel> ();
        private string _searchText;
        
        public EmployeeItems(List<EmployeeModel> employees)
        {
            //Employee = new EmployeeService();

            foreach (EmployeeModel employee in employees)
            {
                _employees.Add(employee);
            }
            
            FilteredEmployees = new ObservableCollection<EmployeeModel>(_employees);
        }
        
        public ObservableCollection<EmployeeModel> FilteredEmployees
        {
            get => _filteredEmployees;
            set
            {
                _filteredEmployees = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterContacts(); // เรียกการกรองเมื่อคำค้นเปลี่ยน
            }
        }

        private void FilterContacts()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredEmployees = new ObservableCollection<EmployeeModel>(_employees);
            }
            else
            {
                var filtered = _employees
                    .Where(c => c.name.ToLower().Contains(SearchText.ToLower()))
                    .ToList();
                FilteredEmployees = new ObservableCollection<EmployeeModel>(filtered);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}