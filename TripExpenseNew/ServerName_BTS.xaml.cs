using CommunityToolkit.Maui.Views;
namespace TripExpenseNew;

public partial class ServerName_BTS : ContentView
{
    public ServerName_BTS()
    {
        InitializeComponent();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert("บันทึก", "ข้อมูลถูกบันทึกเรียบร้อย!", "OK");
        }

        if (this.Parent is CommunityToolkit.Maui.Views.Popup popup)
        {
            // เปลี่ยนจาก DismissAsync เป็น CloseAsync()!
            await popup.CloseAsync(true); // ปิดและส่งค่าว่า "บันทึกแล้ว"
        }
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        if (this.Parent is CommunityToolkit.Maui.Views.Popup popup)
        {
            // เปลี่ยนจาก DismissAsync เป็น CloseAsync()!
            await popup.CloseAsync(false); // ปิดโดยไม่ส่งค่ากลับ
        }
    }

}
    