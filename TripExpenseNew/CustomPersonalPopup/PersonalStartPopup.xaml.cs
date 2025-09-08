using CommunityToolkit.Maui.Views;
using TripExpenseNew.Models;

namespace TripExpenseNew.CustomPersonalPopup;

public partial class PersonalStartPopup : Popup
{
    bool Iscustomer = false;
	public PersonalStartPopup(string location,bool iscustomer,int mileage)
	{
		InitializeComponent();
        Text_Location.Text = location;

        if(location != "CTL(HQ)" && location != "CTL(KBO)" && location != "CTL(RBO)")
        {
            Text_Location.IsEnabled = true;
        }
        else
        {
            Text_Location.IsEnabled = false;
        }

        Iscustomer = iscustomer;
        Text_Mileage.Text = mileage.ToString();
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
        PersonalPopupStartModel personal = new PersonalPopupStartModel()
        {
            IsCustomer = Iscustomer,
            job_id = Text_Job.Text,
            location_name = Text_Location.Text,
            mileage = Text_Mileage.Text != null ? Convert.ToInt32(Text_Mileage.Text) : 0
        };
        Close(personal);
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