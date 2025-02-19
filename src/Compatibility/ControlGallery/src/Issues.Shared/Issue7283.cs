using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7283, "[Android] Crash changing the Application MainPage",
		PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ManualReview)]
#endif
	public class Issue7283 : TestContentPage
	{
		public Issue7283()
		{
			Title = "Issue 7283";
		}

		protected override void Init()
		{
			var layout = new StackLayout
			{
				Padding = new Thickness(12)
			};

			var instructions = new Label
			{
				Text = "Press the Button below. If navigate without any errors, the test has passed."
			};

			var navigateButton = new Button
			{
				Text = "Navigate"
			};

			navigateButton.Clicked += async (sender, e) =>
			{
				navigateButton.IsEnabled = false;

				await Task.Delay(2000);
				var navigation = new NavigationPage();
				Application.Current.MainPage = navigation;
				await Application.Current.MainPage.Navigation.PushAsync(new ContentPage { Title = "Did I crash?" });
			};

			layout.Children.Add(instructions);
			layout.Children.Add(navigateButton);

			Content = layout;
		}
	}
}