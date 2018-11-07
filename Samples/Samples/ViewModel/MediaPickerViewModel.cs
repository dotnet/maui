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
        }

        public ICommand PickPhotoCommand { get; }

        public string MediaPath
        {
            get => mediaPath;
            set => SetProperty(ref mediaPath, value);
        }

        public override void OnAppearing()
        {
            MediaPicker.MediaPicked += OnMediaPicked;
        }

        public override void OnDisappearing()
        {
            MediaPicker.MediaPicked -= OnMediaPicked;
        }

        async void DoPickPhoto()
        {
            await MediaPicker.ShowPhotoPickerAsync();
        }

        void OnMediaPicked(object sender, MediaPickedEventArgs e)
        {
            if (e.IsCanceled)
            {
                DisplayAlertAsync("Media picker CANCELED!");
            }
            else
            {
                MediaPath = e.Path;
            }
        }
    }
}
