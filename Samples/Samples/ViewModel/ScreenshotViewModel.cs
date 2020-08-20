using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    class ScreenshotViewModel : BaseViewModel
    {
        ImageSource screenshot;

        public ScreenshotViewModel()
        {
            ScreenshotCommand = new Command(async () => await CaptureScreenshot());
            EmailCommand = new Command(async () => await EmailScreenshot());
        }

        public ICommand ScreenshotCommand { get; }

        public ICommand EmailCommand { get; }

        public ImageSource Screenshot
        {
            get => screenshot;
            set => SetProperty(ref screenshot, value);
        }

        async Task CaptureScreenshot()
        {
            var mediaFile = await Xamarin.Essentials.Screenshot.CaptureAsync();
            var stream = await mediaFile.OpenReadAsync(ScreenshotFormat.Png);

            Screenshot = ImageSource.FromStream(() => stream);
        }

        async Task EmailScreenshot()
        {
            var mediaFile = await Xamarin.Essentials.Screenshot.CaptureAsync();

            var temp = Path.Combine(FileSystem.CacheDirectory, "screenshot.jpg");
            using (var stream = await mediaFile.OpenReadAsync(ScreenshotFormat.Jpeg))
            using (var file = File.Create(temp))
            {
                await stream.CopyToAsync(file);
            }

            await Email.ComposeAsync(new EmailMessage
            {
                Attachments = { new EmailAttachment(temp) }
            });
        }
    }
}
