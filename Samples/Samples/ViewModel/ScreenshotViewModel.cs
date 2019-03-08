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

        Image screenshot;

        public Image Screenshot
        {
            get => screenshot;
            set => SetProperty(ref screenshot, value);
        }

        public ScreenshotViewModel()
        {
            ScreenshotCommand = new Command(async () => await CaputreScreenshot());
            Screenshot = new Image();
        }

        async Task CaputreScreenshot()
        {
            var mediaFile = await Xamarin.Essentials.Screenshot.CaptureAsync();
            Screenshot.Source = ImageSource.FromFile(mediaFile.Filepath);
            OnPropertyChanged(nameof(Screenshot));
        }
    }
}
