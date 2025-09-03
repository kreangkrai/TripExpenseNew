using CommunityToolkit.Maui.Views;
using TripExpenseNew.Models;

namespace TripExpenseNew.CustomPublicPopup;

public partial class PublicStopPopup : Popup
{
    private bool Iscustomer = false;

    public PublicStopPopup(string location, bool isCustomer)
    {
        InitializeComponent();
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
        Close(null);
    }

    private void OKBtn_Clicked(object sender, EventArgs e)
    {
        PublicPopupStopModel p = new PublicPopupStopModel()
        {
            IsCustomer = Iscustomer,
            location = Text_Location.Text,
        };
        Close(p);
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