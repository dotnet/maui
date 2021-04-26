using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Navigation)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9456, "[Bug] navPage.SetHideNavigationBarSeparator(true) no longer works. ", PlatformAffected.iOS)]
	public class Issue9456 : TestNavigationPage
	{
		protected override void Init()
		{
			BarBackgroundColor = Colors.Blue;
			BarTextColor = Colors.White;

			PlatformConfiguration.iOSSpecific.NavigationPage.SetHideNavigationBarSeparator(this, true);

			Navigation.PushAsync(new Issue9456Page());
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue9456Page : ContentPage
	{
		public Issue9456Page()
		{
			Title = "Issue 9456";

			BackgroundColor = Colors.Blue;

			var layout = new StackLayout
			{
				Padding = 12
			};

			var instructions = new Label
			{
				TextColor = Colors.White,
				Text = "If the NavigationBarSeparator is hidden, the test has passed."
			};

			layout.Children.Add(instructions);

			Content = layout;
		}
	}
}