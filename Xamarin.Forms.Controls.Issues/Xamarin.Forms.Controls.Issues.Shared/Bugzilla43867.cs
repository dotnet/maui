using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 43867, "Numeric keyboard shows text / default keyboard when back button is hit", PlatformAffected.Android)]
	public class Bugzilla43867 : TestContentPage // or TestMasterDetailPage, etc ...
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
						BackgroundColor = Color.AntiqueWhite,
						Keyboard = Keyboard.Numeric
					},
					new Editor
					{
						WidthRequest = 250,
						HeightRequest = 50,
						BackgroundColor = Color.BurlyWood,
						Keyboard = Keyboard.Numeric
					}
				}
			};
		}
	}
}