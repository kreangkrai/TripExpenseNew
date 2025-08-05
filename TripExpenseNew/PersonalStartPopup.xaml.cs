using CommunityToolkit.Maui.Views;
using TripExpenseNew.Models;

namespace TripExpenseNew;

public partial class PersonalStartPopup : Popup
{
	public PersonalStartPopup(string location,bool iscustomer)
	{
		InitializeComponent();
        Text_Location.Text = location;
        chk_customer.IsChecked = iscustomer;
    }

    private void OnSubmitClicked(object sender, EventArgs e)
    {
        PersonalPopupStartModel personal = new PersonalPopupStartModel()
        {
            IsCustomer = chk_customer.IsChecked,
            job = Text_Job.Text,
            location = Text_Location.Text,
            mileage = Text_Mileage.Text != null ? Convert.ToInt32(Text_Mileage.Text) : 0
        };        
        Close(personal);
    }
    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(null);
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {

    }
}