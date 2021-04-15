using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6663, "Fix visibility of hidden pages in the stack", PlatformAffected.Gtk)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Navigation)]
#endif
	public class Issue6663 : ContentPage
	{
		public Issue6663()
		{
			Title = "Issue 6663";
			BackgroundColor = Colors.Green;

			var layout = new StackLayout();

			var instructions = new Label
			{
				Text = "Press the button below to navigate to a new page. Navigate back and verify that the navigation bar is visible and the page is rendered correctly."
			};

			var navigateButton = new Button
			{
				Text = "Navigate"
			};

			navigateButton.Clicked += (sender, e) =>
			{
				Navigation.PushAsync(new Issue663SecondPage());
			};

			layout.Children.Add(instructions);
			layout.Children.Add(navigateButton);

			Content = layout;
		}
	}

	internal class Issue663SecondPage : ContentPage
	{
		public Issue663SecondPage()
		{
			Title = "Issue 6663 SecondPage";
			BackgroundColor = Colors.Red;

			var layout = new StackLayout();

			var instructions = new Label
			{
				Text = "Press the button below to navigate back."
			};

			var navigateButton = new Button
			{
				Text = "Navigate Back"
			};

			navigateButton.Clicked += (sender, e) =>
			{
				Navigation.PopAsync();
			};

			layout.Children.Add(instructions);
			layout.Children.Add(navigateButton);

			Content = layout;
		}
	}
}