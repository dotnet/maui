using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Issue32650 : ContentPage
	{
		public Issue32650()
		{
			InitializeComponent();
		}

		private async void OnPickPhotoClicked(object sender, EventArgs e)
		{
			try
			{
				var options = new MediaPickerOptions
				{
					Title = "Pick a photo",
					RotateImage = true,
					PreserveMetaData = true,
					MaximumWidth = 400
				};

				var photo = await MediaPicker.Default.PickPhotoAsync(options);

				if (photo != null)
				{
					// Check if the returned FileResult is properly oriented
					using var stream = await photo.OpenReadAsync();
					var imageSource = ImageSource.FromStream(() => stream);
					PickedImage.Source = imageSource;
					ResultLabel.Text = $"Photo picked: {photo.FileName}";
				}
				else
				{
					ResultLabel.Text = "Photo pick cancelled";
				}
			}
			catch (Exception ex)
			{
				ResultLabel.Text = $"Error: {ex.Message}";
			}
		}

		private async void OnPickMultiplePhotosClicked(object sender, EventArgs e)
		{
			try
			{
				var options = new MediaPickerOptions
				{
					Title = "Pick photos",
					RotateImage = true,
					PreserveMetaData = true,
					MaximumWidth = 400,
					SelectionLimit = 3
				};

				var photos = await MediaPicker.Default.PickPhotosAsync(options);

				if (photos != null && photos.Count > 0)
				{
					var photo = photos[0];
					using var stream = await photo.OpenReadAsync();
					var imageSource = ImageSource.FromStream(() => stream);
					PickedImage.Source = imageSource;
					ResultLabel.Text = $"Photos picked: {photos.Count} - First: {photo.FileName}";
				}
				else
				{
					ResultLabel.Text = "Photo pick cancelled";
				}
			}
			catch (Exception ex)
			{
				ResultLabel.Text = $"Error: {ex.Message}";
			}
		}

		private async void OnCapturePhotoClicked(object sender, EventArgs e)
		{
			try
			{
				if (!MediaPicker.Default.IsCaptureSupported)
				{
					ResultLabel.Text = "Photo capture not supported on this device";
					return;
				}

				var options = new MediaPickerOptions
				{
					Title = "Take a photo",
					RotateImage = true,
					PreserveMetaData = true,
					MaximumWidth = 400
				};

				var photo = await MediaPicker.Default.CapturePhotoAsync(options);

				if (photo != null)
				{
					using var stream = await photo.OpenReadAsync();
					var imageSource = ImageSource.FromStream(() => stream);
					PickedImage.Source = imageSource;
					ResultLabel.Text = $"Photo captured: {photo.FileName}";
				}
				else
				{
					ResultLabel.Text = "Photo capture cancelled";
				}
			}
			catch (Exception ex)
			{
				ResultLabel.Text = $"Error: {ex.Message}";
			}
		}
	}
}
