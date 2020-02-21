using NUnit.Framework;
using NUnit.Framework.Internal;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Platform.Android;

namespace Xamarin.Forms.ControlGallery.Android.Tests
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
				BackgroundColor = Color.Red
			};

			CheckCornerRadius(boxView);
		}
	
		[Test, Category("CornerRadius"), Category("Button")]
		public void ButtonCornerRadius()
		{
			var backgroundColor = Color.Red;

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
		public void FrameCornerRadius()
		{
			var backgroundColor = Color.Red;

			var frame = new Frame
			{
				HeightRequest = 100,
				WidthRequest = 200,
				CornerRadius = 15,
				BackgroundColor = backgroundColor
			};

			CheckCornerRadius(frame);
		}

		[Test, Category("CornerRadius"), Category("ImageButton")]
		public void ImageButtonCornerRadius()
		{
			var backgroundColor = Color.Red;

			var button = new ImageButton
			{
				HeightRequest = 100,
				WidthRequest = 200,
				CornerRadius = 15,
				BackgroundColor = backgroundColor,
				BorderColor = Color.Black,
				BorderWidth = 2
			};

			CheckCornerRadius(button);
		}

		public void CheckCornerRadius(VisualElement visualElement) 
		{
			using (var renderer = GetRenderer(visualElement))
			{
				var view = renderer.View;
				Layout(visualElement, view);

				// Need to parent the Frame for it to work on lower APIs (below Marshmallow)
				ParentView(view);

				// The corners should show the background color
				view.AssertColorAtTopLeft(EmptyBackground)
					.AssertColorAtTopRight(EmptyBackground)
					.AssertColorAtBottomLeft(EmptyBackground)
					.AssertColorAtBottomRight(EmptyBackground)
					.AssertColorAtCenter(visualElement.BackgroundColor.ToAndroid());

				UnparentView(view);
			}
		}
	}
}