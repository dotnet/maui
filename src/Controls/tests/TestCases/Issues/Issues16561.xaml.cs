using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 16561, "Quick single taps on Android have wrong second tap location", PlatformAffected.Android)]
	public partial class Issue16561 : ContentPage
	{
		int _taps;

		public Issue16561()
		{
			InitializeComponent();

			var tapGesture = new TapGestureRecognizer();
			tapGesture.Tapped += TapHandler;

			TapArea.GestureRecognizers.Add(tapGesture);
		}

		void TapHandler(object sender, TappedEventArgs e)
		{
			var pos = e.GetPosition(TapArea);

			if (pos == null)
			{
				Tap1Label.Text = $"Error, could not get tap position";
				return;
			}

#if ANDROID
			// Adjust the results for display density, so they make sense to the 
			// Appium test consuming this.
			var x = this.Handler.MauiContext.Context.ToPixels(pos.Value.X);
			var y = this.Handler.MauiContext.Context.ToPixels(pos.Value.Y);
			pos = new(x, y);
#endif

			if (_taps % 2 == 0)
			{
				Tap1Label.Text = $"{pos.Value.X}, {pos.Value.Y}";
			}
			else
			{
				Tap2Label.Text = $"{pos.Value.X}, {pos.Value.Y}";
			}

			_taps += 1;
		}
	}
}