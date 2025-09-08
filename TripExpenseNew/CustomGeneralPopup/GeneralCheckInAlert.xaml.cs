using CommunityToolkit.Maui.Views;

namespace TripExpenseNew.CustomGeneralPopup;

public partial class GeneralCheckInAlert : Popup
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(GeneralCheckInAlert), string.Empty);
    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(GeneralCheckInAlert), string.Empty);

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

    public GeneralCheckInAlert()
    {
        InitializeComponent();
    }

    private void Location_Clicked(object sender, EventArgs e)
    {
        Location.IsEnabled = false;
        Close("Location");
        Location.IsEnabled = true;
    }
}