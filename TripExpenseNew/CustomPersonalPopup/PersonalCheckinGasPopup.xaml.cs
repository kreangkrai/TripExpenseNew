using CommunityToolkit.Maui.Views;

namespace TripExpenseNew.CustomPersonalPopup;

public partial class PersonalCheckinGasPopup : Popup
{
	public PersonalCheckinGasPopup()
	{
		InitializeComponent();
	}
    private void OnCloseButtonClicked(object sender, EventArgs e)
    {
        Close(null);
    }
    private void OnOkButtonClicked(object sender, EventArgs e)
    {
        string customer = Text_Gas.Text;
        double cash = Text_Cash.Text == "" ? 0 : Convert.ToDouble(Text_Cash.Text);
        if (customer.Trim() != "" && Text_Gas.Text != null)
        {
            Tuple<string, double> data = new Tuple<string, double>(customer, cash);
            Close(data);
        }
        else
        {
            Close(null);
        }       
    }
}