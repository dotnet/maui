using Microsoft.Maui.Media;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32845, "MediaPicker stops working after launching other ComponentActivity", PlatformAffected.Android)]
public partial class Issue32845 : ContentPage
{

	public Issue32845()
	{
		InitializeComponent();
	}

	private async void OnPickPhotoClicked(object sender, EventArgs e)
	{
		try
		{
			StatusLabel.Text = "Status: Opening picker...";
			var result = await MediaPicker.PickPhotosAsync(new MediaPickerOptions
			{
				Title = "Pick a photo"
			});

			if (result != null && result.Count > 0)
			{
				StatusLabel.Text = $"Status: Photo picked successfully ({result.Count} file(s))";
			}
			else
			{
				StatusLabel.Text = "Status: Picker cancelled";
			}
		}
		catch (Exception ex)
		{
			StatusLabel.Text = $"Status: Error - {ex.Message}";
		}
	}

	private void OnOpenActivityClicked(object sender, EventArgs e)
	{
#if ANDROID
		try
		{
			var intent = new Android.Content.Intent(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity, typeof(SampleComponentActivity));
			Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.StartActivity(intent);
			StatusLabel.Text = "Status: Activity opened";
		}
		catch (Exception ex)
		{
			StatusLabel.Text = $"Status: Error opening activity - {ex.Message}";
		}
#else
		StatusLabel.Text = "Status: Only available on Android";
#endif
	}
}

#if ANDROID
[Android.App.Activity(Label = "Sample Activity", Theme = "@style/Maui.SplashTheme")]
public class SampleComponentActivity : AndroidX.Activity.ComponentActivity
{
	protected override void OnCreate(Android.OS.Bundle savedInstanceState)
	{
		base.OnCreate(savedInstanceState);

		var layout = new Android.Widget.LinearLayout(this)
		{
			Orientation = Android.Widget.Orientation.Vertical
		};
		layout.SetPadding(50, 250, 50, 50);

		var textView = new Android.Widget.TextView(this)
		{
			Text = "This is a ComponentActivity!",
			TextSize = 24,
		};
		textView.SetPadding(0, 0, 0, 50);

		var closeButton = new Android.Widget.Button(this)
		{
			Text = "Close Activity"
		};
		closeButton.Click += (_, _) =>
		{
			Finish();
		};

		layout.AddView(textView);
		layout.AddView(closeButton);

		SetContentView(layout);
	}
}
#endif
