using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Essentials;
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

				Console.WriteLine($"CaptureVideoAsync COMPLETED: {PhotoPath}");
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

		async Task LoadVideoAsync(FileResult video)
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

		public override void OnDisappearing()
		{
			PhotoPath = null;
			VideoPath = null;

			base.OnDisappearing();
		}
	}
}
