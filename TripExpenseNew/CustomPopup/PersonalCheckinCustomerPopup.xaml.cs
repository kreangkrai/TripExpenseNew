using CommunityToolkit.Maui.Views;

namespace TripExpenseNew.CustomPopup;

public partial class PersonalCheckinCustomerPopup : Popup
{
	public PersonalCheckinCustomerPopup(string customer)
	{   
		InitializeComponent();
        Text_Customer.Text = customer;
	}

    private void OnCloseButtonClicked(object sender, EventArgs e)
    {
        Close(null);
    }
    private void OnOKButtonClicked(object sender, EventArgs e)
    {
        string customer = Text_Customer.Text;
        Close(customer);
    }
}