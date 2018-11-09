using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class MediaPickerViewModel : BaseViewModel
    {
        string mediaPath;

        public MediaPickerViewModel()
        {
            PickPhotoCommand = new Command(DoPickPhoto);

            MediaPicker.MediaPicked += OnMediaPicked;
        }

        public ICommand PickPhotoCommand { get; }

        public string MediaPath
        {
            get => mediaPath;
            set => SetProperty(ref mediaPath, value);
        }

        async void DoPickPhoto()
        {
            try
            {
                var photo = await MediaPicker.ShowPhotoPickerAsync();

                // canceled
                if (photo == null)
                {
                    MediaPath = null;
                    return;
                }

                // save the file into local storage
                var newFile = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
                using (var stream = await photo.OpenReadAsync())
                using (var newStream = File.OpenWrite(newFile))
                {
                    await stream.CopyToAsync(newStream);
                }

                MediaPath = newFile;

                Console.WriteLine("ShowPhotoPickerAsync COMPLETED: " + newFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ShowPhotoPickerAsync THREW: " + ex.Message);
            }
        }

        void OnMediaPicked(object sender, MediaPickedEventArgs e)
        {
            if (e.IsCanceled)
            {
                Console.WriteLine("Media picker CANCELED!");
            }
            else
            {
                Console.WriteLine("Media picker PICKED: " + e.File.FilePath);
            }
        }
    }
}
