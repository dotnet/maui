using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            PickCustomTypeCommand = new Command(() => DoPickCustomType());
            PickAndSendCommand = new Command(() => DoPickAndSend());
            PickMultipleFilesCommand = new Command(() => DoPickMultipleFiles());
        }

        public ICommand PickFileCommand { get; }

        public ICommand PickImageCommand { get; }

        public ICommand PickCustomTypeCommand { get; }

        public ICommand PickAndSendCommand { get; }

        public ICommand PickMultipleFilesCommand { get; }

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
                PickerTitle = "Please select an image",
                FileTypes = FilePickerFileType.Images,
            };

            await PickAndShow(options);
        }

        async void DoPickCustomType()
        {
            var customFileType =
                new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.my.comic.extension" } }, // or general UTType values
                    { DevicePlatform.Android, new[] { "application/comics" } },
                    { DevicePlatform.UWP, new[] { ".cbr", ".cbz" } },
                    { DevicePlatform.Tizen, new[] { "*/*" } },
                    { DevicePlatform.macOS, new[] { "cbr", "cbz" } }, // or general UTType values
                });

            var options = new PickOptions
            {
                PickerTitle = "Please select a comic file",
                FileTypes = customFileType,
            };

            await PickAndShow(options);
        }

        async void DoPickAndSend()
        {
            // pick a file
            var result = await PickAndShow(PickOptions.Images);
            if (result == null)
                return;

            // copy it locally
            var copyPath = Path.Combine(FileSystem.CacheDirectory, result.FileName);
            using (var destination = File.Create(copyPath))
            using (var source = await result.OpenReadAsync())
                await source.CopyToAsync(destination);

            // send it via an email
            await Email.ComposeAsync(new EmailMessage
            {
                Subject = "Test Subject",
                Body = "This is the body. There should be an image attached.",
                Attachments =
                {
                    new EmailAttachment(copyPath)
                }
            });
        }

        async Task<FileResult> PickAndShow(PickOptions options)
        {
            try
            {
                var result = await FilePicker.PickAsync(options);

                if (result != null)
                {
                    Text = $"File Name: {result.FileName}";

                    if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                        result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                    {
                        var stream = await result.OpenReadAsync();
                        Image = ImageSource.FromStream(() => stream);
                        IsImageVisible = true;
                    }
                    else
                    {
                        IsImageVisible = false;
                    }
                }
                else
                {
                    Text = $"Pick cancelled.";
                }

                return result;
            }
            catch (Exception ex)
            {
                Text = ex.ToString();
                IsImageVisible = false;
                return null;
            }
        }

        async void DoPickMultipleFiles()
        {
            try
            {
                var resultList = await FilePicker.PickMultipleAsync();

                if (resultList != null && resultList.Any())
                {
                    Text = "File Names: " + string.Join(", ", resultList.Select(result => result.FileName));

                    // only showing the first file's content
                    var firstResult = resultList.First();

                    if (firstResult.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                        firstResult.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                    {
                        var stream = await firstResult.OpenReadAsync();
                        Image = ImageSource.FromStream(() => stream);
                        IsImageVisible = true;
                    }
                    else
                    {
                        IsImageVisible = false;
                    }
                }
                else
                {
                    Text = $"Pick cancelled.";
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
