using CommunityToolkit.Maui.Views;

namespace TripExpenseNew.CustomPersonalPopup;

public partial class CheckInAlert : Popup
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(CheckInAlert), string.Empty);
    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(CheckInAlert), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public CheckInAlert()
    {
        InitializeComponent();
    }

    private void OnCustomerButtonClicked(object sender, EventArgs e)
    {
        Close("Customer");
    }

    private void OnOtherButtonClicked(object sender, EventArgs e)
    {
        Close("Other");
    }

    private void OnGasButtonClicked(object sender, EventArgs e)
    {
        Close("Gas Station");
    }
}