using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    class ScreenshotViewModel : BaseViewModel
    {
        public ICommand ScreenshotCommand { get; }

        ImageSource screenshot;

        public ImageSource Screenshot
        {
            get => screenshot;
            set => SetProperty(ref screenshot, value, onChanged: () => OnPropertyChanged(nameof(Screenshot)));
        }

        public ScreenshotViewModel()
        {
            ScreenshotCommand = new Command(async () => await CaputreScreenshot());
        }

        async Task CaputreScreenshot()
        {
            var mediaFile = await Xamarin.Essentials.Screenshot.CaptureAsync();
            Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
            {
                Screenshot = ImageSource.FromFile(mediaFile.Filepath);
            });
        }
    }
}
