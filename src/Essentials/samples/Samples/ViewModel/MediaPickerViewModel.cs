using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;

namespace Samples.ViewModel
{
	public class MediaPickerViewModel : BaseViewModel
	{
		ImageSource photoSource;
		ImageSource videoSource;

		bool showPhoto;
		bool showVideo;

		public MediaPickerViewModel()
		{
			PickPhotoCommand = new Command(DoPickPhoto);
			CapturePhotoCommand = new Command(DoCapturePhoto, () => MediaPicker.IsCaptureSupported);

			PickVideoCommand = new Command(DoPickVideo);
			CaptureVideoCommand = new Command(DoCaptureVideo, () => MediaPicker.IsCaptureSupported);
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

		public ImageSource PhotoSource
		{
			get => photoSource;
			set => SetProperty(ref photoSource, value);
		}

		public ImageSource VideoSource
		{
			get => videoSource;
			set => SetProperty(ref videoSource, value);
		}

		async void DoPickPhoto()
		{
			try
			{
				var photo = await MediaPicker.PickPhotoAsync();

				await LoadPhotoAsync(photo);

				Console.WriteLine($"PickPhotoAsync COMPLETED: {PhotoSource}");
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

				Console.WriteLine($"CapturePhotoAsync COMPLETED: {PhotoSource}");
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

				Console.WriteLine($"PickVideoAsync COMPLETED: {PhotoSource}");
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

				Console.WriteLine($"CaptureVideoAsync COMPLETED: {VideoSource}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"CaptureVideoAsync THREW: {ex.Message}");
			}
		}

		async Task LoadPhotoAsync(FileResult photo)
		{
			// canceled
			if (photo == null)
			{
				PhotoSource = null;
				return;
			}

			var stream = await photo.OpenReadAsync();
			PhotoSource = ImageSource.FromStream(() => stream);

			ShowVideo = false;
			ShowPhoto = true;
		}

		async Task LoadVideoAsync(FileResult video)
		{
			// canceled
			if (video == null)
			{
				VideoSource = null;
				return;
			}

			var stream = await video.OpenReadAsync();
			VideoSource = ImageSource.FromStream(() => stream);

			ShowVideo = true;
			ShowPhoto = false;
		}

		public override void OnDisappearing()
		{
			PhotoSource = null;
			VideoSource = null;

			base.OnDisappearing();
		}
	}
}
