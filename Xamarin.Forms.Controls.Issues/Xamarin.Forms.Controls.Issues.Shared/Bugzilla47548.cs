using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 47548, "Setting soft input mode to resize creates gap", PlatformAffected.Android)]
	public class Bugzilla47548 : TestContentPage 
	{
		static string GetMode()
		{
			return Application.Current.On<Android>().GetWindowSoftInputModeAdjust() == WindowSoftInputModeAdjust.Pan
				? "Pan"
				: "Resize";
		}

		protected override void Init()
		{
			var button = new Button() { Text = $"Toggle Soft Input Mode (Currently {GetMode()})"};

			button.Clicked += (sender, args) =>
			{
				Application.Current.On<Android>()
					.UseWindowSoftInputModeAdjust(Application.Current.On<Android>().GetWindowSoftInputModeAdjust() ==
					                              WindowSoftInputModeAdjust.Pan
						? WindowSoftInputModeAdjust.Resize
						: WindowSoftInputModeAdjust.Pan);
				
				button.Text = $"Toggle Soft Input Mode (Currently {GetMode()})";
			};

			Content = new StackLayout
			{
				BackgroundColor = Color.CadetBlue,
				Spacing = 10,
				VerticalOptions = LayoutOptions.Fill,
				Children =
				{
					new Label
					{
						Text = @"With Soft Input Mode set to Pan, tapping the Entry at the bottom of the screen should cause the whole page to scroll up above the keyboard.
With Soft Input Mode set to Resize, tapping the Entry at the bottom of the screen should resize the content to display everything above the keyboard (the Crimson Label in the middle should be squashed to fit)."
					},
					button,
					new Label
					{
						FontSize = 12f,
						HeightRequest = 500,
						Text = @"Meh",
						BackgroundColor = Color.Crimson
					},
					new Entry()
				}
			};
		}
	}
}