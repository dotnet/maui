using Microsoft.Maui.Media;

namespace Maui.Controls.Sample.Issues;
public partial class Issue31075Modal : ContentPage
{
    public event EventHandler ModalClosed;

    public Issue31075Modal()
    {
        InitializeComponent();
    }

    async void OnTestMediaPickerClicked(object sender, System.EventArgs e)
    {
        try
        {
            ResultLabel.Text = "Testing photo capture...";
            
            // Test the MediaPicker that was previously causing modal dismissal
            var result = await MediaPicker.CapturePhotoAsync();
            
            if (result != null)
            {
                ResultLabel.Text = "Photo captured successfully!";
            }
            else
            {
                ResultLabel.Text = "Photo capture was cancelled";
            }
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"Error: {ex.Message}";
        }
    }

    async void OnTestVideoPickerClicked(object sender, System.EventArgs e)
    {
        try
        {
            ResultLabel.Text = "Testing video capture...";
            
            // Test the MediaPicker that was previously causing modal dismissal
            var result = await MediaPicker.CaptureVideoAsync();
            
            if (result != null)
            {
                ResultLabel.Text = "Video captured successfully!";
            }
            else
            {
                ResultLabel.Text = "Video capture was cancelled";
            }
        }
        catch (Exception ex)
        {
            ResultLabel.Text = $"Error: {ex.Message}";
        }
    }

    async void OnCloseModalClicked(object sender, System.EventArgs e)
    {
        ModalClosed?.Invoke(this, EventArgs.Empty);
        await Navigation.PopModalAsync();
    }
}