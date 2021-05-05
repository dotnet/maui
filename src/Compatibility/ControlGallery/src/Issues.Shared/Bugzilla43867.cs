using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 43867, "Numeric keyboard shows text / default keyboard when back button is hit", PlatformAffected.Android)]
	public class Bugzilla43867 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			Application.Current.On<Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);

			Content = new StackLayout
			{
				Spacing = 10,
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = "Focus and unfocus each element 10 times using the Back button. Observe that the soft keyboard does not show different characters while hiding. Now repeat the test by tapping off of the element."
					},
					new Entry
					{
						WidthRequest = 250,
						HeightRequest = 50,
						BackgroundColor = Colors.AntiqueWhite,
						Keyboard = Keyboard.Numeric
					},
					new Editor
					{
						WidthRequest = 250,
						HeightRequest = 50,
						BackgroundColor = Colors.BurlyWood,
						Keyboard = Keyboard.Numeric
					}
				}
			};
		}
	}
}