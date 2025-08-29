using CommunityToolkit.Maui.Views;

namespace TripExpenseNew.CustomPopup;

public partial class CompanyCheckinCustomerPopup : Popup
{
    public CompanyCheckinCustomerPopup(string customer)
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