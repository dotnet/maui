using NUnit.Framework;
using Xamarin.UITest;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("Slider")]
	internal class SliderGalleryTests : BaseTestFixture
	{
		// TODO: Detect Slider value changes
		// TODO: Port to new conventions

		public SliderGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.SliderGalleryLegacy);
		}
//		void AllElementsPresent ()
//		{
//			var sliders = App.Query (PlatformQueries.Sliders);
//			Assert.AreEqual (3, sliders.Length);

//			App.Screenshot ("All Sliders present");
//		}

		[Test]
		[Description ("Slide all Sliders, test ValueChanged event")]
		public void SliderGallerySlideAllSliders ()
		{
//			AllElementsPresent ();

//			var sliders = App.Query (PlatformQueries.Sliders);
//			var sliderLeft = sliders[0].Rect.X;
//			var sliderRight = sliderLeft + sliders[0].Rect.Width - 5;  // Needed to move 5 pixels left so that the drag would register
//			var topSliderY = sliders[0].Rect.CenterY;
//			var middleSliderY = sliders[1].Rect.CenterY;
//			var bottomSliderY = sliders[2].Rect.CenterY;

//			// Move top slider, numbers should change
//			App.DragFromTo (sliderLeft, topSliderY, sliderRight, topSliderY);
//			App.WaitForElement (q => q.Marked ("100"), "Timeout : 100");
//			App.Screenshot ("Move first slider right");

//			App.DragFromTo (sliderRight, topSliderY, sliderLeft, topSliderY);
//			App.WaitForElement (q => q.Marked ("20"), "Timeout : 20");
//			App.Screenshot ("Move first slider left");

//			// Move middle slider, shouldn't move
//			App.DragFromTo (sliderLeft, middleSliderY, sliderRight, middleSliderY);
//			App.WaitForElement (q => q.Marked ("20"), "Timeout : 20");
//			App.Screenshot ("Tried to move disabled slider");

//			// Move bottom slider, should move but nothing happens
//			App.DragFromTo (sliderLeft, bottomSliderY, sliderRight, bottomSliderY);
//			App.WaitForElement (q => q.Marked ("20"), "Timeout : 20");
//			App.Screenshot ("Move third slider right");

//			App.DragFromTo (sliderRight, bottomSliderY, sliderLeft, bottomSliderY);
//			App.WaitForElement (q => q.Marked ("20"), "Timeout : 20");
//			App.Screenshot ("Move first slider left");

		}

//		[Test]
//		public void AllElementsPresentLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			AllElementsPresent ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Slide all Sliders - Landscape")]
//		public void SliderGallerySlideAllSlidersLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			SliderGallerySlideAllSliders ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

	}
}
