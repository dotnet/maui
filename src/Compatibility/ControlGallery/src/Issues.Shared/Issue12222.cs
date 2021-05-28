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
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12222, "[Bug] [MacOS] Buttons don't render correctly when given a HeightRequest", PlatformAffected.macOS)]
	public class Issue12222 : TestContentPage
	{
		public Issue12222()
		{
		}

		protected override void Init()
		{
			var layout = new StackLayout();

			var defaultLabel = new Label
			{
				Text = "Default Button"
			};

			var defaultButton = new Button
			{
				Text = "Button"
			};

			var heightLabel = new Label
			{
				Text = "Button using a custom HeightRequest (60)"
			};

			var heightButton = new Button
			{
				HeightRequest = 60,
				Text = "Button"
			};

			var backgroundLabel = new Label
			{
				Text = "Button using a custom HeightRequest (60)"
			};

			var backgroundButton = new Button
			{
				HeightRequest = 60,
				BackgroundColor = Colors.OrangeRed,
				Text = "Button"
			};

			var borderLabel = new Label
			{
				Text = "Button using a custom HeightRequest (60)"
			};

			var borderButton = new Button
			{
				HeightRequest = 60,
				BackgroundColor = Colors.OrangeRed,
				BorderColor = Colors.YellowGreen,
				BorderWidth = 2,
				Text = "Button"
			};

			layout.Children.Add(defaultLabel);
			layout.Children.Add(defaultButton);

			layout.Children.Add(heightLabel);
			layout.Children.Add(heightButton);

			layout.Children.Add(backgroundButton);
			layout.Children.Add(backgroundButton);

			layout.Children.Add(borderLabel);
			layout.Children.Add(borderButton);

			Content = layout;
		}
	}
}
