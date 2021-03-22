using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1332, "Frame inside frame does not resize after visibility changed", PlatformAffected.Android)]
	public class Issue1332 : TestContentPage
	{
		protected override void Init()
		{
			double layoutWidth = 0.6;
			double layoutHeight = 150;

			var red = new Frame
			{
				BackgroundColor = Color.Red,
				Content = new Frame
				{
					BorderColor = Color.Black,
					HeightRequest = layoutHeight,
					BackgroundColor = Color.Transparent
				}
			};
			AbsoluteLayout.SetLayoutBounds(red, new Rectangle(0, 0, layoutWidth, layoutHeight));
			AbsoluteLayout.SetLayoutFlags(red, AbsoluteLayoutFlags.XProportional | AbsoluteLayoutFlags.WidthProportional);

			var stack = new StackLayout
			{
				Children =
				{
					new Button
					{
						Text = "visibility",
						Padding = 10,
						Command = new Command(() => red.IsVisible = !red.IsVisible)
					},
					new Button
					{
						Text = "width",
						Padding = 10,
						Command = new Command(() => {
							layoutWidth = layoutWidth == 0.3 ? 0.6 : 0.3;
							red.IsVisible = false;
							AbsoluteLayout.SetLayoutBounds(red, new Rectangle(0, 0, layoutWidth, 150));
							red.IsVisible = true;
						})
					}
				}
			};
			AbsoluteLayout.SetLayoutBounds(stack, new Rectangle(1, 0, layoutWidth / 2, 1));
			AbsoluteLayout.SetLayoutFlags(stack, AbsoluteLayoutFlags.All);

			var desc = new Label
			{
				Text = "Click on Visibility, then click on Width button few times." +
					"The layout of the second frame must be updated to match the first."
			};
			AbsoluteLayout.SetLayoutBounds(desc, new Rectangle(0, 0.5, 1, 0.5));
			AbsoluteLayout.SetLayoutFlags(desc, AbsoluteLayoutFlags.All);

			Content = new AbsoluteLayout
			{
				Children =
				{
					red,
					stack,
					desc
				}
			};
		}
	}
}
