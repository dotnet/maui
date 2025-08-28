using System.Reflection;
using Microsoft.Maui.Graphics.Platform;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 30426, "IImage Downsize() throws Exception in 9.0.81 when called from background thread on iOS", PlatformAffected.iOS)]
    public partial class Issue30426 : TestContentPage
    {
        private IImage _originalImage;

        public Issue30426()
        {
            InitializeComponent();
        }

        protected override void Init()
        {

        }

        private async void OnSelectImageClicked(object sender, EventArgs e)
        {
            await LoadEmbeddedImageAsync();
        }

        private async Task LoadEmbeddedImageAsync()
        {
            try
            {
                var assembly = GetType().GetTypeInfo().Assembly;
                using (var stream = assembly.GetManifestResourceStream("Controls.TestCases.HostApp.Resources.Images.royals.png"))
                {
                    _originalImage = PlatformImage.FromStream(stream);
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);

                    FileSizeLabel.Text = $"File Size: {stream.Length / 1024:N0} KB";
                    DimensionsLabel.Text = $"Dimensions: {_originalImage.Width} x {_originalImage.Height}";
                }

                ProcessImageButton.IsEnabled = true;
                StatusLabel.Text = "Status: Image loaded successfully";
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load image: {ex.Message}");
            }
        }

        private void OnResizeSliderChanged(object sender, ValueChangedEventArgs e)
        {
            var percentage = (int)e.NewValue;
            ResizeLabel.Text = $"{percentage}%";
        }

        private async void OnProcessImageClicked(object sender, EventArgs e)
        {
            if (_originalImage == null)
            {
                ShowError("No image loaded");
                return;
            }

            try
            {
                StatusLabel.Text = "Status: Processing image...";
                ErrorLabel.IsVisible = false;
                ProcessImageButton.IsEnabled = false;

                var resizePercentage = ResizeSlider.Value / 100f;
                var newWidth = (float)(_originalImage.Width * resizePercentage);
                var newHeight = (float)(_originalImage.Height * resizePercentage);

                // This should cause the issue on iOS 9.0.81 when called from background thread
                await Task.Run(() => ProcessImageOnBackgroundThread(newWidth, newHeight));

            }
            catch (Exception ex)
            {
                ShowError($"Image processing failed: {ex.Message}");
            }
            finally
            {
                ProcessImageButton.IsEnabled = true;
            }
        }

        private void ProcessImageOnBackgroundThread(float newWidth, float newHeight)
        {
            try
            {
                // This call should throw UIKitThreadAccessException on iOS 9.0.81
                var resizedImage = _originalImage.Downsize(newWidth, newHeight, false);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdateImageDisplay(resizedImage, "Background Thread");
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ShowError($"Background thread processing failed: {ex.Message}");
                });
            }
        }

        private async void UpdateImageDisplay(IImage resizedImage, string processingMethod)
        {
            try
            {
                using var stream = new MemoryStream();
                await resizedImage.SaveAsync(stream, ImageFormat.Jpeg, 0.8f);

                stream.Position = 0;
                DisplayImage.Source = ImageSource.FromStream(() => new MemoryStream(stream.ToArray()));

                FileSizeLabel.Text = $"File Size: {stream.Length / 1024:N0} KB";
                DimensionsLabel.Text = $"Dimensions: {resizedImage.Width} x {resizedImage.Height}";
                StatusLabel.Text = $"Status: Image processed successfully";
            }
            catch (Exception ex)
            {
                ShowError($"Failed to update display: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            ErrorLabel.Text = message;
            ErrorLabel.IsVisible = true;
            StatusLabel.Text = "Status: Error occurred";
        }
    }
}
 