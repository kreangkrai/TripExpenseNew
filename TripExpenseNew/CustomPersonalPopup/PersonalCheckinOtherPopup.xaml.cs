using CommunityToolkit.Maui.Views;

namespace TripExpenseNew.CustomPersonalPopup;

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
        CancelBtn.IsEnabled = false;
        Close(null);
        CancelBtn.IsEnabled = true;
    }

    private void OnOkButtonClicked(object sender, EventArgs e)
    {
        OKBtn.IsEnabled = false;
        string location = Text_Other.Text;
        Close(location);
        OKBtn.IsEnabled = true;
    }
}