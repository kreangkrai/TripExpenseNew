using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TripExpenseNew.ViewModels;
using ZXing;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using ZXing.QrCode.Internal;
using Image = SixLabors.ImageSharp.Image;
using CommunityToolkit.Maui.Media;
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
    }
    private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            foreach (var barcode in e.Results)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Confirm.IsEnabled = true;
                    Confirm.TextColor = Colors.White;
                    Confirm.BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#297CC0");
                    Confirm.Text = barcode.Value;
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
            // iOS ยังใช้ Permissions.Photos
            var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Photos>();
                if (status != PermissionStatus.Granted)
                {
                    ReadQRBtn.IsEnabled = true;
                    return;
                }
            }
#endif

            // ใช้ MediaPicker (รองรับ Android Photo Picker โดยอัตโนมัติใน MAUI 8+)
            var pickResult = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Please Select QR Code"
            });

            if (pickResult == null)
            {
                Close(null);
                ReadQRBtn.IsEnabled = true;
                return;
            }

            // เปิด stream จากไฟล์ที่เลือก
            using var stream = await pickResult.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            if (memoryStream.Length == 0)
            {
                Close(null);
                ReadQRBtn.IsEnabled = true;
                return;
            }

            // โหลดภาพด้วย ImageSharp
            using var image = Image.Load<Rgba32>(memoryStream);

            if (image.Width == 0 || image.Height == 0)
            {
                Close(null);
                ReadQRBtn.IsEnabled = true;
                return;
            }

            // ปรับขนาดภาพ
            using var resizedImage = image.Clone();
            resizedImage.Mutate(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new SixLabors.ImageSharp.Size(1000, 1000),
                Mode = SixLabors.ImageSharp.Processing.ResizeMode.Max
            }));

            // แปลงเป็น RGB bytes
            var rgbBytes = new byte[resizedImage.Width * resizedImage.Height * 3];
            int index = 0;
            resizedImage.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < accessor.Width; x++)
                    {
                        rgbBytes[index++] = row[x].R;
                        rgbBytes[index++] = row[x].G;
                        rgbBytes[index++] = row[x].B;
                    }
                }
            });

            // อ่าน QR Code
            var luminanceSource = new RGBLuminanceSource(rgbBytes, resizedImage.Width, resizedImage.Height);
            var reader = new BarcodeReaderGeneric
            {
                Options = new ZXing.Common.DecodingOptions
                {
                    PossibleFormats = new[] { ZXing.BarcodeFormat.QR_CODE },
                    TryHarder = true
                }
            };

            var result = reader.Decode(luminanceSource);

            Close(result?.Text);
        }
        catch
        {
            Close(null);
        }
        finally
        {
            ReadQRBtn.IsEnabled = true;
        }
    }

    private void Cancel_Clicked(object sender, EventArgs e)
    {
        Cancel.IsEnabled = false;
        Close(null);
        Cancel.IsEnabled = true;
    }
}