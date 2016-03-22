using NUnit.Framework;
using Xamarin.UITest;
using System;
using System.Threading;
using Xamarin.UITest.Queries;
using System.Diagnostics;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("AbsoluteLayout")]
	internal class AbsoluteLayoutGalleryTests : BaseTestFixture
	{
		// TODO: Port to new conventions

		public AbsoluteLayoutGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.AbsoluteLayoutGalleryLegacy);
		}

		[Test]
		public void AbsoluteLayoutGalleryMoveBox ()
		{
			App.Screenshot ("At Gallery");

			//	App.WaitForElement (q => q.Raw (Views.BoxView), "Timeout : Box Renderers");
			//	Assert.AreEqual (1, App.Query (q => q.Raw (Views.BoxView)).Length);

			//	App.WaitForElement (q => q.Raw (Views.Label), "Timeout : Labels");
			//	Assert.AreEqual (4, App.Query(q => q.Raw (Views.Label)).Length);

			//	App.WaitForElement (q => q.Raw (Views.Slider), "Timeout : Sliders");
			//	Assert.AreEqual (4, App.Query(q => q.Raw (Views.Slider)).Length);
			//	App.Screenshot ("All elements exist");

			//	App.WaitForElement (q => q.Raw (Views.Slider));
			//	// Move green box left
			//	var sliders = (App.Query (q => q.Raw (Views.Slider)));
			//	var sliderCenter = (sliders[0]).Rect.CenterX;
			//	var sliderLeft = (sliders[0]).Rect.X;
			//	var sliderRight = sliderLeft + (sliders[0]).Rect.Width;

			//	var xSlider = (sliders[0]).Rect.CenterY;
			//	var ySlider = (sliders[1]).Rect.CenterY;

			//var absoluteBounds = App.Query (PlatformQueries.AbsoluteGalleryBackground)[0].Rect;

			//// Move box left
			//App.DragFromTo (sliderCenter, xSlider, sliderLeft, xSlider, Speed.Slow);
			//Assert.AreEqual (absoluteBounds.X, (App.Query (PlatformQueries.BoxRendererQuery))[0].Rect.X, 2.0);
			//App.Screenshot ("Box moved to left bounds");

			//// Move box right
			//App.DragFromTo (sliderLeft, xSlider, sliderRight, xSlider, Speed.Slow);
			//Assert.AreEqual (absoluteBounds.X + absoluteBounds.Width, (App.Query (PlatformQueries.BoxRendererQuery))[0].Rect.X + (App.Query (PlatformQueries.BoxRendererQuery))[0].Rect.Width, 2.0);
			//App.Screenshot ("Box moved to right bounds");

			////Move box up
			//var boxContainer = App.Query (PlatformQueries.AbsoluteGalleryBackground)[0];
			//var boxContainerTop = boxContainer.Rect.Y;
			//var boxContainerBottom = boxContainer.Rect.Y + boxContainer.Rect.Height;

			//App.DragFromTo (sliderCenter, ySlider, sliderLeft, ySlider, Speed.Slow);
			//Assert.AreEqual (boxContainerTop, (App.Query (PlatformQueries.BoxRendererQuery))[0].Rect.Y);
			//App.Screenshot ("Box moved to top bounds");

			//// Move box down
			//App.DragFromTo (sliderLeft, ySlider, sliderRight, ySlider, Speed.Slow);
			//Assert.AreEqual (boxContainerBottom, (App.Query (PlatformQueries.BoxRendererQuery))[0].Rect.Y + (App.Query (PlatformQueries.BoxRendererQuery))[0].Rect.Height);
			//App.Screenshot ("Box moved to bottom bounds");
		}

		//[Test] 
		//[Description ("MaxWidth")]
		//public void AbsoluteLayoutGalleryResizeToMaximumWidth ()
		//{
			//App.WaitForElement (PlatformQueries.Sliders, "Timeout : Sliders");

			//AppRect widthSlider = (App.Query (PlatformQueries.Sliders))[2].Rect;
			//float sliderCenter = widthSlider.CenterX;
			//float sliderY = widthSlider.CenterY;
			//float sliderRight = widthSlider.X + widthSlider.Width;

			//App.DragFromTo (sliderCenter, sliderY, sliderRight, sliderY, Speed.Slow);

			//AppRect absoluteBounds = App.Query (PlatformQueries.AbsoluteGalleryBackground)[0].Rect;
			//AppRect boxBounds = App.Query (PlatformQueries.BoxRendererQuery)[0].Rect;

			//Assert.AreEqual (absoluteBounds.Width, boxBounds.Width, 2.0);
			//App.Screenshot ("Box at maximum width");
		//}

		//[Test] 
		//[Description ("MaxHeight")]
		//public void AbsoluteLayoutGalleryResizeToMaximumHeight ()
		//{
			//App.WaitForElement (PlatformQueries.Sliders, "Timeout : Sliders");

			//AppRect widthSlider = (App.Query (PlatformQueries.Sliders))[3].Rect;
			//float sliderCenter = widthSlider.CenterX;
			//float sliderY = widthSlider.CenterY;
			//float sliderRight = widthSlider.X + widthSlider.Width;

			//App.DragFromTo (sliderCenter, sliderY, sliderRight, sliderY, Speed.Slow);

			//AppRect absoluteBounds = App.Query (PlatformQueries.AbsoluteGalleryBackground)[0].Rect;
			//AppRect boxBounds = App.Query (PlatformQueries.BoxRendererQuery)[0].Rect;

			//Assert.AreEqual (absoluteBounds.Height, boxBounds.Height, 2.0);
			//App.Screenshot ("Box at maximum height");
		//}

		//[Test] 
		//[Description ("MinWidth")]
		//public void AbsoluteLayoutGalleryResizeToMinimumWidth ()
		//{
			//App.WaitForElement (PlatformQueries.Sliders, "Timeout : Sliders");

			//AppRect widthSlider = (App.Query (PlatformQueries.Sliders))[2].Rect;
			//float sliderCenter = widthSlider.CenterX;
			//float sliderY = widthSlider.CenterY;
			//float sliderLeft = widthSlider.X - 20;

			//App.DragFromTo (sliderCenter, sliderY, sliderLeft, sliderY, Speed.Slow);

			//bool isZeroed = false;
			//if (App.Query (PlatformQueries.BoxRendererQuery).Length == 0) {
			//	// Android removes 0 width BoxView
			//	isZeroed = true;
			//} else {
			//	if (App.Query (PlatformQueries.BoxRendererQuery)[0].Rect.Width <= 4.0)
			//		isZeroed = true;
			//}

			//Assert.IsTrue (isZeroed, "Box is minimum width");
			//App.Screenshot ("Box at minimum width");
		}

		//[Test] 
		//[Description ("MinHeight")]
		//public void AbsoluteLayoutGalleryResizeToMinimumHeight ()
		//{
			//App.WaitForElement (PlatformQueries.Sliders, "Timeout : Sliders");

			//AppRect widthSlider = (App.Query (PlatformQueries.Sliders))[3].Rect;
			//float sliderCenter = widthSlider.CenterX;
			//float sliderY = widthSlider.CenterY;
			//float sliderLeft = widthSlider.X - 20;

			//App.DragFromTo (sliderCenter, sliderY, sliderLeft, sliderY, Speed.Slow);

			//bool isZeroed = false;
			//if (App.Query (PlatformQueries.BoxRendererQuery).Length == 0) {
			//	// Android removes 0 height BoxView
			//	isZeroed = true;
			//} else {
			//	if (App.Query (PlatformQueries.BoxRendererQuery)[0].Rect.Height <= 4.0)
			//		isZeroed = true;
			//}

			//Assert.IsTrue (isZeroed, "Box is minimum height");
			//App.Screenshot ("Box at minimum height");
		//}
			
/*******************************************************/
/**************** Landscape tests **********************/
/*******************************************************/

		//[Test]
		//[Description ("Move box around with sliders - landscape")]
		//public void AbsoluteLayoutGalleryMoveBoxLandscape ()
		//{
		//	App.SetOrientationLandscape ();
		//	App.Screenshot ("Rotated to Landscape");
		//	AbsoluteLayoutGalleryMoveBox ();
		//	App.SetOrientationPortrait ();
		//	App.Screenshot ("Rotated to Portrait");
		//}

		//[Test]
		//[Description ("Resize to max width with sliders - landscape")]
		//public void AbsoluteLayoutGalleryResizeToMaximumWidthLandscape ()
		//{
		//	App.SetOrientationLandscape ();
		//	App.Screenshot ("Rotated to Landscape");
		//	AbsoluteLayoutGalleryResizeToMaximumWidth ();
		//	App.SetOrientationPortrait ();
		//	App.Screenshot ("Rotated to Portrait");
		//}

		//[Test]
		//[Description ("Resize to max height with sliders - landscape")]
		//public void AbsoluteLayoutGalleryResizeToMaximumHeightLandscape ()
		//{
		//	App.SetOrientationLandscape ();
		//	App.Screenshot ("Rotated to Landscape");
		//	AbsoluteLayoutGalleryResizeToMaximumHeight ();
		//	App.SetOrientationPortrait ();
		//	App.Screenshot ("Rotated to Portrait");
		//}

		//[Test]
		//[Description ("Resize to min height with sliders - landscape")]
		//public void AbsoluteLayoutGalleryResizeToMinimumWidthLandscape ()
		//{
		//	App.SetOrientationLandscape ();
		//	App.Screenshot ("Rotated to Landscape");
		//	AbsoluteLayoutGalleryResizeToMinimumWidth ();
		//	App.SetOrientationPortrait ();
		//	App.Screenshot ("Rotated to Portrait");
		//}

		//[Test]
		//[Description ("Resize to min height with sliders - landscape")]
		//public void AbsoluteLayoutGalleryResizeToMinimumHeightLandscape ()
		//{
		//	App.SetOrientationLandscape ();
		//	App.Screenshot ("Rotated to Landscape");
		//	AbsoluteLayoutGalleryResizeToMinimumHeight ();
		//	App.SetOrientationPortrait ();
		//	App.Screenshot ("Rotated to Portrait");
		//}
	//}
}
