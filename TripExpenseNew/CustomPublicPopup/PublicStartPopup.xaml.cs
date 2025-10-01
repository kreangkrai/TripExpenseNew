using CommunityToolkit.Maui.Views;
using TripExpenseNew.Models;

namespace TripExpenseNew.CustomPublicPopup;

public partial class PublicStartPopup : Popup
{
    bool Iscustomer = false;
    string location = string.Empty;
    bool iscustomer = false;
    public PublicStartPopup(string _location, bool _iscustomer)
    {
        InitializeComponent();
        location = _location;
        iscustomer = _iscustomer;
    }

    private void CancelBtn_Clicked(object sender, EventArgs e)
    {
        CancelBtn.IsEnabled = false;
        Close(null);
        CancelBtn.IsEnabled = true;
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

        Iscustomer = iscustomer;
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
    private void OKBtn_Clicked(object sender, EventArgs e)
    {
        OKBtn.IsEnabled = false;
        PublicPopupStartModel p = new PublicPopupStartModel()
        {
            IsCustomer = Iscustomer,
            job_id = Text_Job.Text,
            location_name = Text_Location.Text,
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