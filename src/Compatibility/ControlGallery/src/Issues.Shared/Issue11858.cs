using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11858, "Ellipse is not antialised",
		PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shape)]
#endif
	public class Issue11858 : TestContentPage
	{
		public Issue11858()
		{
		}

		protected override void Init()
		{
			Title = "Issue 11858";

			var layout = new StackLayout();

			var instructions = new Label
			{
				Padding = 12,
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				Text = "If despite scaling the ellipse looks sharp, the test has passed."
			};

			var ellipse = new Ellipse
			{
				HorizontalOptions = LayoutOptions.Start,
				HeightRequest = 100,
				WidthRequest = 100,
				Scale = 2,
				StrokeThickness = 12,
				Stroke = Brush.Red,
				Margin = new Thickness(100, 100, 0, 0)
			};

			layout.Children.Add(instructions);
			layout.Children.Add(ellipse);

			Content = layout;
		}
	}
}