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
        employees = employees.Where(w=> !emps.Contains(w.emp_id)).ToList();

        BindingContext = new EmployeeItems(employees);
    }
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        
    }
    private void CancelBtn_Clicked(object sender, EventArgs e)
    {
        Close(null);
    }

    private void AddBtn_Clicked(object sender, EventArgs e)
    {
        //Close("059197");
    }

    private void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // ตรวจสอบว่ามีการเลือก item หรือไม่
        if (e.CurrentSelection.FirstOrDefault() is EmployeeModel emp)
        {
            //bool confirm = await DisplayAlert("Confirm Drop Off", $"Drop Off: {passengerItem.TextPassenger}?", "Yes", "No");
            Close(emp);
        }
    }
}