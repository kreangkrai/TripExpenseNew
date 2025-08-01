using CommunityToolkit.Maui.Views;
using System.Threading.Tasks;
namespace TripExpenseNew;

public partial class CheckInPopup : Popup
{
	public CheckInPopup()
	{
		InitializeComponent();
	}
    private void OnSubmitClicked(object sender, EventArgs e)
    {
        Close(TextEntry.Text);
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(null);
    }
}