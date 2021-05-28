using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5395, "[macOs] Image Rotation issue", PlatformAffected.macOS)]
	public class Issue5395 : ContentPage
	{
		public Issue5395()
		{
			var sl = new StackLayout() { Orientation = StackOrientation.Vertical };

			sl.Children.Add(new Label() { Text = "Image should rotate clockwise around its center" });
			sl.Children.Add(new TestImage(0.5, 0.5, true, false));

			sl.Children.Add(new Label() { Text = "Image should rotate clockwise around its top left corner" });
			sl.Children.Add(new TestImage(0, 0, true, false));

			sl.Children.Add(new Label() { Text = "Image should rotate clockwise around its top right corner" });
			sl.Children.Add(new TestImage(1, 0, true, false));

			sl.Children.Add(new Label() { Text = "Image should rotate clockwise around its bottom right corner" });
			sl.Children.Add(new TestImage(1, 1, true, false));

			sl.Children.Add(new Label() { Text = "Image should scale on its center" });
			sl.Children.Add(new TestImage(0.5, 0.5, false, true));

			Content = sl;
		}
		class TestImage : Image
		{
			public TestImage(double anchorx, double anchory, bool rotate, bool scale)
			{
				VerticalOptions = HorizontalOptions = LayoutOptions.Center;
				Aspect = Aspect.AspectFit;
				Source = "bank.png";
				WidthRequest = 30;
				HeightRequest = 30;
				BackgroundColor = Colors.Red;
				AnchorX = anchorx;
				AnchorY = anchory;
				//TranslationX = -50;
				//TranslationY = 25;
				if (rotate)
				{
					this.RotateTo(3600, 10000);
				}
				if (scale)
				{
					this.ScaleTo(2, 4000);
				}
				Margin = 30;
			}
		}
	}
}

