using CommunityToolkit.Maui.Views;

namespace TripExpenseNew.CustomPopup;

public partial class PersonalCheckinOtherPopup : Popup
{
	public PersonalCheckinOtherPopup(string customer)
	{
		InitializeComponent();
        Text_Other.Text = customer;
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