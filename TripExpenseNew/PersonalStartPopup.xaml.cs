using CommunityToolkit.Maui.Views;
using TripExpenseNew.Models;

namespace TripExpenseNew;

public partial class PersonalStartPopup : Popup
{
    bool Iscustomer = false;
	public PersonalStartPopup(string location,bool iscustomer)
	{
		InitializeComponent();
        Text_Location.Text = location;
        Iscustomer = iscustomer;
        if (Iscustomer)
        {
            CustomerBtn.BackgroundColor = Colors.Blue;
            OtherBtn.BackgroundColor = Colors.Grey;
        }
        else
        {
            CustomerBtn.BackgroundColor = Colors.Grey;
            OtherBtn.BackgroundColor = Colors.Blue;
        }
    }

    private void CancelBtn_Clicked(object sender, EventArgs e)
    {
        Close(null);
    }

    private void OKBtn_Clicked(object sender, EventArgs e)
    {
        PersonalPopupStartModel personal = new PersonalPopupStartModel()
        {
            IsCustomer = Iscustomer,
            job = Text_Job.Text,
            location = Text_Location.Text,
            mileage = Text_Mileage.Text != null ? Convert.ToInt32(Text_Mileage.Text) : 0
        };
        Close(personal);
    }

    private void CustomerBtn_Clicked(object sender, EventArgs e)
    {
        Iscustomer = true;

        CustomerBtn.BackgroundColor = Colors.Blue;
        OtherBtn.BackgroundColor = Colors.Grey;
    }

    private void OtherBtn_Clicked(object sender, EventArgs e)
    {
        Iscustomer = false;
        CustomerBtn.BackgroundColor = Colors.Grey;
        OtherBtn.BackgroundColor = Colors.Blue;
    }
}