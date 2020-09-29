using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Frame)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11291,
		"[Bug] IsClippedToBounds not clipping Image when inside a Frame",
		PlatformAffected.iOS)]
	public class Issue11291 : TestContentPage
	{
		public Issue11291()
		{

		}

		protected override void Init()
		{
			Title = "Issue 11291";

			var layout = new StackLayout();

			var instructions = new Label
			{
				Padding = 12,
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "If the image clips with the border of the Frame, the test has passed."
			};

			var frame = new Frame
			{
				IsClippedToBounds = true,
				BorderColor = Color.Black,
				Padding = 0,
				CornerRadius = 24,
				Margin = 12
			};

			var image = new Image
			{
				Aspect = Aspect.AspectFill,
				Source = "oasis.jpg"
			};

			frame.Content = image;

			layout.Children.Add(instructions);
			layout.Children.Add(frame);

			Content = layout;
		}
	}
}