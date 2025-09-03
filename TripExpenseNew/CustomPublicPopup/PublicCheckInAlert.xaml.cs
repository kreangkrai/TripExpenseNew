using CommunityToolkit.Maui.Views;

namespace TripExpenseNew.CustomPublicPopup;

public partial class PublicCheckInAlert : Popup
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(PublicCheckInAlert), string.Empty);
    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(PublicCheckInAlert), string.Empty);

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

    public PublicCheckInAlert()
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
}