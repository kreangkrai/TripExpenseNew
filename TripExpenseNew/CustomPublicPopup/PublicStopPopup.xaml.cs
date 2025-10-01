using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Devices.Sensors;
using TripExpenseNew.Models;

namespace TripExpenseNew.CustomPublicPopup;

public partial class PublicStopPopup : Popup
{
    private bool Iscustomer = false;
    string location = string.Empty;
    bool isCustomer = false;
    public PublicStopPopup(string _location, bool _isCustomer)
    {
        InitializeComponent();
        location = _location;
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
        PublicPopupStopModel p = new PublicPopupStopModel()
        {
            IsCustomer = Iscustomer,
            location = Text_Location.Text,
        };
        Close(p);
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