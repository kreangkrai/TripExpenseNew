using CommunityToolkit.Maui.Views;
using System.Threading.Tasks;
using TripExpenseNew.Interface;
using TripExpenseNew.Models;
using TripExpenseNew.Services;
using TripExpenseNew.ViewModels;

namespace TripExpenseNew.PassengerPage;

public partial class PersonalPassengerPopup : Popup
{
    private IEmployee Employee;
    private ILastTrip LastTrip;
    EmployeeModel _emp = new EmployeeModel();
    List<EmployeeModel> employees = new List<EmployeeModel>();
    public PersonalPassengerPopup()
    {
        InitializeComponent();
    }
    protected async override void OnParentChanged()
    {
        base.OnParentChanged();
        Employee = new EmployeeService();
        LastTrip = new LastTripService();
        employees = await Employee.GetEmployees();

        List<string> emps = await LastTrip.GetInUse();
        employees = employees.Where(w => !emps.Contains(w.emp_id)).ToList();

        BindingContext = new EmployeeItems(employees);
    }
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

    }
    private void CancelBtn_Clicked(object sender, EventArgs e)
    {
        CancelBtn.IsEnabled = false;
        Close(null);
        CancelBtn.IsEnabled = true;
    }

    private void AddBtn_Clicked(object sender, EventArgs e)
    {
        AddBtn.IsEnabled = false;
        Close(_emp);
        AddBtn.IsEnabled = true;
    }

    private void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // ตรวจสอบว่ามีการเลือก item หรือไม่
        if (e.CurrentSelection.FirstOrDefault() is EmployeeModel emp)
        {
            _emp = emp;
            AddBtn.IsEnabled = true;
        }
    }
}