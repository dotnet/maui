using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		int pickerSelectionLimit = 1;
		private ObservableCollection<ImageSource> photoList = [];
		private bool showMultiplePhotos;

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

		public int PickerSelectionLimit
		{
			get => pickerSelectionLimit;
			set => SetProperty(ref pickerSelectionLimit, value);
		}

		public bool ShowPhoto
		{
			get => showPhoto;
			set => SetProperty(ref showPhoto, value);
		}

		public bool ShowMultiplePhotos
		{
			get => showMultiplePhotos;
			set => SetProperty(ref showMultiplePhotos, value);
		}

		public bool ShowVideo
		{
			get => showVideo;
			set => SetProperty(ref showVideo, value);
		}

		public ObservableCollection<ImageSource> PhotoList
		{
			get => photoList;
			set => SetProperty(ref photoList, value);
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
				var photo = await MediaPicker.PickPhotosAsync(new MediaPickerOptions
				{
					Title = "Pick a photo",
					SelectionLimit = PickerSelectionLimit,
				});

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
				var video = await MediaPicker.PickVideoAsync(new MediaPickerOptions
				{
					Title = "Pick a video",
					SelectionLimit = PickerSelectionLimit,
				});

				await LoadVideoAsync(video);

				Console.WriteLine($"PickVideoAsync COMPLETED: {VideoSource}");
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
			if (photo is null)
			{
				PhotoSource = null;
				return;
			}

			var stream = await photo.OpenReadAsync();
			PhotoSource = ImageSource.FromStream(() => stream);

			ShowVideo = false;
			ShowMultiplePhotos = false;
			ShowPhoto = true;
		}

		async Task LoadPhotoAsync(List<FileResult> photo)
		{
			PhotoList.Clear();

			// canceled
			if (photo is null || photo.Count == 0)
			{
				PhotoSource = null;
				return;
			}

			foreach (var item in photo)
			{
				var stream = await item.OpenReadAsync();
				PhotoList.Add(ImageSource.FromStream(() => stream));
			}

			ShowVideo = false;
			ShowPhoto = false;
			ShowMultiplePhotos = true;
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
			PhotoList?.Clear();
			PhotoSource = null;
			VideoSource = null;

			base.OnDisappearing();
		}
	}
}
