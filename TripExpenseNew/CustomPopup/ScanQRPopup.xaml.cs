using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TripExpenseNew.ViewModels;
using ZXing;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using Image = SixLabors.ImageSharp.Image;
namespace TripExpenseNew.CustomPopup;

public partial class ScanQRPopup : Popup
{
    public ScanQRPopup()
	{
		InitializeComponent();
              
    }
    protected override void OnParentChanged()
    {
        base.OnParentChanged();

        cameraBarcodeReaderView.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.TwoDimensional,
            AutoRotate = true,
            Multiple = true,
            TryHarder = true
        };
        //cameraBarcodeReaderView.Focus(new Microsoft.Maui.Graphics.Point (200,200));
    }
    private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            foreach (var barcode in e.Results)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (BindingContext is ButtonScanQRResult viewModel)
                    {
                        viewModel.ButtonScanQRResultText = barcode.Value;
                        Confirm.BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#297CC0");
                        Confirm.IsEnabled = true;
                    }
                    else
                    {
                        Confirm.Text = barcode.Value;
                        Confirm.BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#297CC0");
                        Confirm.IsEnabled = true;
                    }
                });
                
            }
        });
    }

    private void Confirm_Clicked(object sender, EventArgs e)
    {
        Confirm.IsEnabled = false;
        Close(Confirm.Text);
        Confirm.IsEnabled = true;
    }

    private async void ReadQRBtn_Clicked(object sender, EventArgs e)
    {
        ReadQRBtn.IsEnabled = false;
        try
        {

#if IOS

            var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Photos>();
                if (status != PermissionStatus.Granted)
                {                   
                    return;
                }
            }
#elif ANDROID
        var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.StorageRead>();
                if (status != PermissionStatus.Granted)
                {                   
                    return;
                }
            }
#endif
            var fileResult = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Please Select QR Code",
                FileTypes = FilePickerFileType.Images
            });

            if (fileResult == null)
                return;
           
            // แสดงภาพที่เลือก
            //SelectedImage.Source = ImageSource.FromFile(fileResult.FullPath);

            // อ่านไฟล์ภาพเป็น Stream
            using var stream = await fileResult.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            // ตรวจสอบว่า stream มีข้อมูล
            if (memoryStream.Length == 0)
            {                
                return;
            }

            // โหลดภาพด้วย ImageSharp
            using var image = Image.Load<Rgba32>(memoryStream);

            // ตรวจสอบขนาดภาพ
            if (image.Width == 0 || image.Height == 0)
            {
                return;
            }

            // ปรับขนาดภาพ (ถ้าจำเป็น)
            using var resizedImage = image.Clone(); // สร้างสำเนาก่อน
            resizedImage.Mutate(ctx => ctx.Resize(1000, 1000));

            // แปลงภาพเป็น array ของ RGB bytes (ไม่รวม Alpha)
            var rgbBytes = new byte[resizedImage.Width * resizedImage.Height * 3];
            int index = 0;
            resizedImage.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < accessor.Width; x++)
                    {
                        rgbBytes[index++] = row[x].R; // Red
                        rgbBytes[index++] = row[x].G; // Green
                        rgbBytes[index++] = row[x].B; // Blue
                    }
                }
            });

            // สร้าง LuminanceSource
            var luminanceSource = new RGBLuminanceSource(rgbBytes, resizedImage.Width, resizedImage.Height);

            // ใช้ BarcodeReaderGeneric เพื่ออ่านบาร์โค้ด
            var reader = new BarcodeReaderGeneric
            {
                Options = new ZXing.Common.DecodingOptions
                {
                    PossibleFormats = new[] { ZXing.BarcodeFormat.All_1D, ZXing.BarcodeFormat.QR_CODE }, // รองรับทุกประเภท
                    //AutoRotate = true,
                    TryHarder = true
                }
            };

            // อ่านบาร์โค้ดจากภาพ
            var result = reader.Decode(luminanceSource);

            // แสดงผลลัพธ์
            if (result != null)
            {
                Close(result.Text);
            }
            else
            {
                Close(null);
            }
        }
        catch
        {
            Close(null);
        }
        ReadQRBtn.IsEnabled = true;
    }

    private void Cancel_Clicked(object sender, EventArgs e)
    {
        Cancel.IsEnabled = false;
        Close(null);
        Cancel.IsEnabled = true;
    }
}