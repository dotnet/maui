using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class FilePickerViewModel : BaseViewModel
    {
        string text;

        ImageSource image;

        bool isImageVisible;

        public FilePickerViewModel()
        {
            PickFileCommand = new Command(() => DoPickFile());
            PickImageCommand = new Command(() => DoPickImage());
        }

        public ICommand PickFileCommand { get; }

        public ICommand PickImageCommand { get; }

        public string Text
        {
            get => text;
            set => SetProperty(ref text, value);
        }

        public ImageSource Image
        {
            get => image;
            set => SetProperty(ref image, value);
        }

        public bool IsImageVisible
        {
            get => isImageVisible;
            set => SetProperty(ref isImageVisible, value);
        }

        async void DoPickFile()
        {
            await PickAndShow(PickOptions.Default);
        }

        async void DoPickImage()
        {
            var options = new PickOptions
            {
                FileTypes = new string[] { FilePickerFileTypes.Png, FilePickerFileTypes.Jpg },
                PickerTitle = "Please select an image"
            };

            await PickAndShow(PickOptions.Images);
        }

        async Task PickAndShow(PickOptions options)
        {
            try
            {
                var result = await FilePicker.PickFileAsync(options);

                if (result != null)
                {
                    Text = $"Name: {result.FileName}, URI: {result.FileUri}";

                    if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                        result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                    {
                        Image = ImageSource.FromStream(() => result.GetStream());
                        IsImageVisible = true;
                    }
                    else
                    {
                        IsImageVisible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Text = ex.ToString();
                IsImageVisible = false;
            }
        }
    }
}
