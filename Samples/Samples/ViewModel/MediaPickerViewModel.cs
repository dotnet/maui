using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class MediaPickerViewModel : BaseViewModel
    {
        string photoPath;
        string videoPath;

        bool showPhoto;
        bool showVideo;

        public MediaPickerViewModel()
        {
            PickPhotoCommand = new Command(DoPickPhoto);
            CapturePhotoCommand = new Command(DoCapturePhoto);

            PickVideoCommand = new Command(DoPickVideo);
            CaptureVideoCommand = new Command(DoCaptureVideo);
        }

        public ICommand PickPhotoCommand { get; }

        public ICommand CapturePhotoCommand { get; }

        public ICommand PickVideoCommand { get; }

        public ICommand CaptureVideoCommand { get; }

        public bool ShowPhoto
        {
            get => showPhoto;
            set => SetProperty(ref showPhoto, value);
        }

        public bool ShowVideo
        {
            get => showVideo;
            set => SetProperty(ref showVideo, value);
        }

        public string PhotoPath
        {
            get => photoPath;
            set => SetProperty(ref photoPath, value);
        }

        public string VideoPath
        {
            get => videoPath;
            set => SetProperty(ref videoPath, value);
        }

        async void DoPickPhoto()
        {
            try
            {
                var photo = await MediaPicker.PickPhotoAsync();

                await LoadPhotoAsync(photo);

                Console.WriteLine($"PickPhotoAsync COMPLETED: {PhotoPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PickPhotoAsync THREW: {ex.Message}");
            }
        }

        async void DoCapturePhoto()
        {
            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();

                await LoadPhotoAsync(photo);

                Console.WriteLine($"CapturePhotoAsync COMPLETED: {PhotoPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CapturePhotoAsync THREW: {ex.Message}");
            }
        }

        async void DoPickVideo()
        {
            try
            {
                var video = await MediaPicker.PickVideoAsync();

                await LoadVideoAsync(video);

                Console.WriteLine($"PickVideoAsync COMPLETED: {PhotoPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PickVideoAsync THREW: {ex.Message}");
            }
        }

        async void DoCaptureVideo()
        {
            try
            {
                var photo = await MediaPicker.CaptureVideoAsync();

                await LoadVideoAsync(photo);

                Console.WriteLine($"CapturePhotoAsync COMPLETED: {PhotoPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CapturePhotoAsync THREW: {ex.Message}");
            }
        }

        async Task LoadPhotoAsync(MediaPickerResult photo)
        {
            // canceled
            if (photo == null)
            {
                PhotoPath = null;
                return;
            }

            // save the file into local storage
            var newFile = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using (var stream = await photo.OpenReadAsync())
            using (var newStream = File.OpenWrite(newFile))
            {
                await stream.CopyToAsync(newStream);
            }

            PhotoPath = newFile;
            ShowVideo = false;
            ShowPhoto = true;
        }

        async Task LoadVideoAsync(MediaPickerResult video)
        {
            // canceled
            if (video == null)
            {
                VideoPath = null;
                return;
            }

            // save the file into local storage
            var newFile = Path.Combine(FileSystem.CacheDirectory, video.FileName);
            using (var stream = await video.OpenReadAsync())
            using (var newStream = File.OpenWrite(newFile))
            {
                await stream.CopyToAsync(newStream);
            }

            VideoPath = newFile;
            ShowVideo = true;
            ShowPhoto = false;
        }
    }
}
