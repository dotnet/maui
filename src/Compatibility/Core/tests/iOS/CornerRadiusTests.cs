using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS.UnitTests
{
	[TestFixture]
	public class CornerRadiusTests : PlatformTestFixture
	{
		[Test, Category("CornerRadius"), Category("BoxView")]
		public async Task BoxviewCornerRadius()
		{
			var boxView = new BoxView
			{
				HeightRequest = 100,
				WidthRequest = 200,
				CornerRadius = 15,
				BackgroundColor = Color.CadetBlue
			};

			await CheckCornerRadius(boxView);
		}

		[Test, Category("CornerRadius"), Category("Button")]
		public async Task ButtonCornerRadius()
		{
			var backgroundColor = Color.CadetBlue;

			var button = new Button
			{
				HeightRequest = 100,
				WidthRequest = 200,
				CornerRadius = 15,
				BackgroundColor = backgroundColor
			};

			await CheckCornerRadius(button);
		}

		[Test, Category("CornerRadius"), Category("Frame")]
		public async Task FrameCornerRadius()
		{
			var backgroundColor = Color.CadetBlue;

			var frame = new Frame
			{
				HeightRequest = 100,
				WidthRequest = 200,
				CornerRadius = 40,
				BackgroundColor = backgroundColor,
				BorderColor = Color.Brown,
				Content = new Label { Text = "Hey" }
			};

			await CheckCornerRadius(frame);
		}

		[Test, Category("CornerRadius"), Category("ImageButton")]
		public async Task ImageButtonCornerRadius()
		{
			var backgroundColor = Color.CadetBlue;

			var button = new ImageButton
			{
				HeightRequest = 100,
				WidthRequest = 200,
				CornerRadius = 15,
				BackgroundColor = backgroundColor
			};

			await CheckCornerRadius(button);
		}

		async Task CheckCornerRadius(View view)
		{
			var page = new ContentPage() { Content = view };
			var centerColor = view.BackgroundColor.ToUIColor();

			var screenshot = await GetRendererProperty(view, (ver) => ver.NativeView.ToBitmap(), requiresLayout: true);
			screenshot.AssertColorAtCenter(centerColor)
			.AssertColorAtBottomLeft(EmptyBackground)
							.AssertColorAtBottomRight(EmptyBackground)
							.AssertColorAtTopLeft(EmptyBackground)
							.AssertColorAtTopRight(EmptyBackground);
		}
	}
}