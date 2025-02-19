using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8701, "TalkBack reads labeled back button as 'Unlabeled button'", PlatformAffected.Android)]
	public class Issue8701 : TestNavigationPage
	{
		protected override void Init()
		{
			Navigation.PushAsync(new Issue8701FirstPage());
		}

		class Issue8701FirstPage : ContentPage
		{
			public Issue8701FirstPage()
			{
				Title = "Issue 8701 first page";
				SetBackButtonTitle(this, "Back button title is set");

				Button button = new Button() { Text = "Navigate to the second page" };
				button.Clicked += async (sender, args) => await Navigation.PushAsync(new Issue8701SecondPage());

				Content = new StackLayout()
				{
					Margin = 20,

					Children =
					{
						button
					}
				};

			}
		}

		class Issue8701SecondPage : ContentPage
		{
			public Issue8701SecondPage()
			{
				Title = "Issue 8701 second page";

				Label label = new Label() { Text = "Enable TalkBack and tap on the back button. Test passes if TalkBack reads: 'Back button title is set'" };

				Content = new StackLayout()
				{
					Margin = 20,

					Children =
					{
						label
					}
				};
			}
		}
	}
}