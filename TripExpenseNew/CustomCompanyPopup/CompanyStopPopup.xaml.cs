using CommunityToolkit.Maui.Views;
using TripExpenseNew.Models;

namespace TripExpenseNew.CustomCompanyPopup;

public partial class CompanyStopPopup : Popup
{
    private bool Iscustomer = false;
    string location = string.Empty;
    bool isCustomer = false;
    int mileage = 0;
    string car_id = string.Empty;

    public CompanyStopPopup(string _location, bool _isCustomer, int _mileage, string _car_id)
    {
        InitializeComponent();
        location = _location;
        mileage = _mileage;
        car_id = _car_id;
        isCustomer = _isCustomer;
    }
    protected override void OnParentChanged()
    {
        base.OnParentChanged();
        Text_Location.Text = location;
        if (location != "CTL(HQ)" && location != "CTL(KBO)" && location != "CTL(RBO)")
        {
            Text_Location.IsEnabled = true;
        }
        else
        {
            Text_Location.IsEnabled = false;
        }

        Iscustomer = isCustomer;
        Text_mileage.Text = $"Mileage Start: {mileage}";
        //Text_Car.Text = car_id;
        Text_Car.Text = $"Car: {car_id}";
        if (Iscustomer)
        {
            CustomerBtn.BackgroundColor = Color.FromArgb("#297CC0");
            OtherBtn.BackgroundColor = Colors.Grey;
        }
        else
        {
            CustomerBtn.BackgroundColor = Colors.Grey;
            OtherBtn.BackgroundColor = Color.FromArgb("#297CC0");
        }
    }
    private void CancelBtn_Clicked(object sender, EventArgs e)
    {
        CancelBtn.IsEnabled = false;
        Close(null);
        CancelBtn.IsEnabled = true;
    }

    private void OKBtn_Clicked(object sender, EventArgs e)
    {
        OKBtn.IsEnabled = false;
        CompanyPopupStopModel company = new CompanyPopupStopModel()
        {
            IsCustomer = Iscustomer,
            location = Text_Location.Text,
            mileage = Text_Mileage.Text != null ? Convert.ToInt32(Text_Mileage.Text) : 0,
            car_id = car_id
        };
        Close(company);
        OKBtn.IsEnabled = true;
    }

    private void CustomerBtn_Clicked(object sender, EventArgs e)
    {
        Iscustomer = true;
        CustomerBtn.BackgroundColor = Color.FromArgb("#297CC0");
        OtherBtn.BackgroundColor = Colors.Grey;
    }

    private void OtherBtn_Clicked(object sender, EventArgs e)
    {
        Iscustomer = false;
        CustomerBtn.BackgroundColor = Colors.Grey;
        OtherBtn.BackgroundColor = Color.FromArgb("#297CC0");
    }
}