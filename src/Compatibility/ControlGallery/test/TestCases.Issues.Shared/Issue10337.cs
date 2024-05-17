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
	[Category(UITestCategories.Navigation)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10337, "[Bug]iOS Renderer NavigationBar.ShadowImage = new UIImage() not remove shadow line after xamarin.forms 4.5", PlatformAffected.iOS)]
	public class Issue10337 : TestNavigationPage
	{
		public Issue10337()
		{
			var page = new ContentPage
			{
				Title = "Issue 10337"
			};

			var layout = new StackLayout();

			var navigateButton = new Button
			{
				Text = "Navigate using Custom NavigationPage"
			};

			navigateButton.Clicked += (sender, args) =>
			{
				Navigation.PushAsync(new Issue10337NavigationPage(new Issue10337()));
			};

			layout.Children.Add(navigateButton);

			page.Content = layout;

			PushAsync(page);
		}

		protected override void Init()
		{

		}
	}

	[Preserve(AllMembers = true)]
	public class Issue10337NavigationPage : NavigationPage
	{
		public Issue10337NavigationPage()
		{

		}

		public Issue10337NavigationPage(Page root) : base(root)
		{

		}
	}
}