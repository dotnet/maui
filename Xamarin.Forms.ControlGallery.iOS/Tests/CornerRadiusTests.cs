using NUnit.Framework;
using Xamarin.Forms.Platform.iOS;

namespace Xamarin.Forms.ControlGallery.iOS.Tests
{
	[TestFixture]
	public class CornerRadiusTests : PlatformTestFixture
	{
		[Test, Category("CornerRadius"), Category("BoxView")]
		public void BoxviewCornerRadius()
		{
			var boxView = new BoxView
			{
				HeightRequest = 100,
				WidthRequest = 200,
				CornerRadius = 15,
				BackgroundColor = Color.CadetBlue
			};

			CheckCornerRadius(boxView);
		}

		[Test, Category("CornerRadius"), Category("Button")]
		public void ButtonCornerRadius()
		{
			var backgroundColor = Color.CadetBlue;

			var button = new Button
			{
				HeightRequest = 100,
				WidthRequest = 200,
				CornerRadius = 15,
				BackgroundColor = backgroundColor
			};

			CheckCornerRadius(button);
		}

		[Test, Category("CornerRadius"), Category("Frame")]
		[Ignore("Will not pass until https://github.com/xamarin/Xamarin.Forms/issues/9265 is fixed")]
		public void FrameCornerRadius()
		{
			var backgroundColor = Color.CadetBlue;

			var frame = new Frame
			{
				HeightRequest = 100,
				WidthRequest = 200,
				CornerRadius = 15,
				BackgroundColor = backgroundColor,
				BorderColor = Color.Brown,
				Content = new Label { Text = "Hey" }
			};

			CheckCornerRadius(frame);
		}

		[Test, Category("CornerRadius"), Category("ImageButton")]
		public void ImageButtonCornerRadius()
		{
			var backgroundColor = Color.CadetBlue;

			var button = new ImageButton
			{
				HeightRequest = 100,
				WidthRequest = 200,
				CornerRadius = 15,
				BackgroundColor = backgroundColor
			};

			CheckCornerRadius(button);
		}

		public void CheckCornerRadius(View view)
		{
			var page = new ContentPage() { Content = view };

			using (var pageRenderer = GetRenderer(page))
			{
				using (var uiView = GetRenderer(view).NativeView)
				{
					page.Layout(new Rectangle(0, 0, view.WidthRequest, view.HeightRequest));

				
						uiView
							.AssertColorAtCenter(view.BackgroundColor.ToUIColor())
							.AssertColorAtBottomLeft(EmptyBackground)
							.AssertColorAtBottomRight(EmptyBackground)
							.AssertColorAtTopLeft(EmptyBackground)
							.AssertColorAtTopRight(EmptyBackground);
					
				}
			}
		}
	}
}