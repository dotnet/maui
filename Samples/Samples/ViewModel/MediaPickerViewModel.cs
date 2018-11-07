using System;
using System.Collections.Generic;
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
                MediaPath = photo.Path;

                Console.WriteLine("ShowPhotoPickerAsync COMPLETED: " + photo.Path);
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
                Console.WriteLine("Media picker PICKED: " + e.Path);
            }
        }
    }
}
