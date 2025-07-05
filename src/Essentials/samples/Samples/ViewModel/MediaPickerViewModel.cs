using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;

namespace Samples.ViewModel
{
	public class PhotoInfo
	{
		public ImageSource Source { get; set; }
		public string Dimensions { get; set; }
		public string FileSize { get; set; }
	}

	public class MediaPickerViewModel : BaseViewModel
	{
		ImageSource photoSource;

		bool showPhoto;
		int pickerSelectionLimit = 1;
		int pickerCompressionQuality = 100;
		int pickerMaximumWidth = 0;
		int pickerMaximumHeight = 0;
		bool pickerRotateImage = false;
		bool pickerPreserveMetaData = true;
		long imageByteLength = 0;
		string imageDimensions = "";
		private ObservableCollection<PhotoInfo> photoList = [];
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

		public int PickerCompressionQuality
		{
			get => pickerCompressionQuality;
			set => SetProperty(ref pickerCompressionQuality, value);
		}

		public int PickerMaximumWidth
		{
			get => pickerMaximumWidth;
			set => SetProperty(ref pickerMaximumWidth, value);
		}

		public int PickerMaximumHeight
		{
			get => pickerMaximumHeight;
			set => SetProperty(ref pickerMaximumHeight, value);
		}

		public bool PickerRotateImage
		{
			get => pickerRotateImage;
			set => SetProperty(ref pickerRotateImage, value);
		}

		public bool PickerPreserveMetaData
		{
			get => pickerPreserveMetaData;
			set => SetProperty(ref pickerPreserveMetaData, value);
		}

		public long ImageByteLength
		{
			get => imageByteLength;
			set => SetProperty(ref imageByteLength, value);
		}

		public string ImageDimensions
		{
			get => imageDimensions;
			set => SetProperty(ref imageDimensions, value);
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

		public ObservableCollection<PhotoInfo> PhotoList
		{
			get => photoList;
			set => SetProperty(ref photoList, value);
		}

		public ImageSource PhotoSource
		{
			get => photoSource;
			set => SetProperty(ref photoSource, value);
		}

		async void DoPickPhoto()
		{
			try
			{
				var photo = await MediaPicker.PickPhotosAsync(new MediaPickerOptions
				{
					Title = "Pick a photo",
					SelectionLimit = PickerSelectionLimit,
					CompressionQuality = PickerCompressionQuality,
					MaximumWidth = PickerMaximumWidth > 0 ? PickerMaximumWidth : null,
					MaximumHeight = PickerMaximumHeight > 0 ? PickerMaximumHeight : null,
					RotateImage = PickerRotateImage,
					PreserveMetaData = PickerPreserveMetaData
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
				var photo = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
				{
					Title = "Capture a photo",
					CompressionQuality = PickerCompressionQuality,
					MaximumWidth = PickerMaximumWidth > 0 ? PickerMaximumWidth : null,
					MaximumHeight = PickerMaximumHeight > 0 ? PickerMaximumHeight : null,
					RotateImage = PickerRotateImage,
					PreserveMetaData = PickerPreserveMetaData
				});

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
				var videos = await MediaPicker.PickVideosAsync(new MediaPickerOptions
				{
					Title = "Pick a video",
					SelectionLimit = PickerSelectionLimit,
					RotateImage = PickerRotateImage,
					PreserveMetaData = PickerPreserveMetaData
				});

				ShowPhoto = false;
				ShowMultiplePhotos = false;
				ImageByteLength = 0;
				ImageDimensions = "";

				await DisplayAlertAsync($"{videos.Count} videos successfully picked.");

				Console.WriteLine($"PickVideosAsync COMPLETED: {videos.Count} selected");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"PickVideosAsync THREW: {ex.Message}");
			}
		}

		async void DoCaptureVideo()
		{
			try
			{
				var video = await MediaPicker.CaptureVideoAsync();

				ShowPhoto = false;
				ShowMultiplePhotos = false;
				ImageByteLength = 0;
				ImageDimensions = "";

				await DisplayAlertAsync($"Video successfully captured at {video.FullPath}.");

				Console.WriteLine($"CaptureVideoAsync COMPLETED: {video.FullPath}");
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
				ImageDimensions = "";
				return;
			}

			var stream = await photo.OpenReadAsync();

			// Get image dimensions
			try
			{
				var imageInfo = GetImageDimensions(stream);
				ImageDimensions = $"{imageInfo.Width} × {imageInfo.Height} • {stream.Length:N0} bytes";
				stream.Position = 0; // Reset stream position
			}
			catch
			{
				ImageDimensions = $"Unknown dimensions • {stream.Length:N0} bytes";
			}

			PhotoSource = ImageSource.FromStream(() => stream);
			ImageByteLength = stream.Length;

			ShowMultiplePhotos = false;
			ShowPhoto = true;
		}

		async Task LoadPhotoAsync(List<FileResult> photo)
		{
			PhotoList.Clear();
			ImageByteLength = 0;
			ImageDimensions = "";

			// canceled
			if (photo is null || photo.Count == 0)
			{
				PhotoSource = null;
				return;
			}

			foreach (var item in photo)
			{
				var stream = await item.OpenReadAsync();

				// Get image dimensions
				var dimensions = GetImageDimensions(stream);
				stream.Position = 0; // Reset stream position for ImageSource

				var photoInfo = new PhotoInfo
				{
					Source = ImageSource.FromStream(() => stream),
					Dimensions = $"{dimensions.Width} × {dimensions.Height}",
					FileSize = $"{stream.Length:N0} bytes"
				};

				PhotoList.Add(photoInfo);
				ImageByteLength += stream.Length;
			}

			// Show count for multiple photos
			ImageDimensions = $"{photo.Count} photos selected";

			ShowPhoto = false;
			ShowMultiplePhotos = true;
		}

		(int Width, int Height) GetImageDimensions(Stream imageStream)
		{
			try
			{
				// Reset position to beginning of stream
				imageStream.Position = 0;

				// Use MAUI.Graphics to load the image and get dimensions
				using var image = PlatformImage.FromStream(imageStream);
				return ((int)image.Width, (int)image.Height);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Failed to extract image dimensions: {ex.Message}");
				return (0, 0);
			}
		}

		public override void OnDisappearing()
		{
			PhotoList?.Clear();
			PhotoSource = null;
			ImageDimensions = "";

			base.OnDisappearing();
		}
	}
}
