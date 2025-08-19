using CommunityToolkit.Maui.Views;

namespace TripExpenseNew.CustomPopup;

public partial class PersonalCheckinOtherPopup : Popup
{
	public PersonalCheckinOtherPopup(string customer)
	{
		InitializeComponent();
        Text_Other.Text = customer;
        if (customer != "CTL(HQ)" && customer != "CTL(KBO)" && customer != "CTL(RBO)")
        {
            Text_Other.IsEnabled = true;
        }
        else
        {
            Text_Other.IsEnabled = false;
        }
    }

    private void OnCloseButtonClicked(object sender, EventArgs e)
    {
        Close(null);
    }

    private void OnOkButtonClicked(object sender, EventArgs e)
    {
        string location = Text_Other.Text;
        Close(location);
    }
}