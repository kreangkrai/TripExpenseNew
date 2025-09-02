using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Devices.Sensors;

namespace TripExpenseNew.CustomPersonalPopup;

public partial class PersonalCheckinCustomerPopup : Popup
{
	public PersonalCheckinCustomerPopup(string customer)
	{   
		InitializeComponent();
        Text_Customer.Text = customer;
        if (customer != "CTL(HQ)" && customer != "CTL(KBO)" && customer != "CTL(RBO)")
        {
            Text_Customer.IsEnabled = true;
        }
        else
        {
            Text_Customer.IsEnabled = false;
        }
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