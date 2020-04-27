using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;

namespace Xamarin.Forms.Core.UITests
{
	[Category(UITestCategories.CarouselView)]
	internal class CarouselViewUITests : BaseTestFixture
	{
		string _carouselViewGalleries = "CarouselView Galleries";

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.CollectionViewGallery);

			App.WaitForElement(_carouselViewGalleries);
			App.Tap(_carouselViewGalleries);
		}

		[TestCase("CarouselView (XAML, Horizontal)")]
		public void CarouselViewRemoveAndUpdateCurrentItem(string subgallery)
		{
			VisitSubGallery(subgallery);

			CheckPositionValue("lblPosition", "0");
			CheckPositionValue("lblCurrentItem", "0");
			CheckPositionValue("lblSelected", "0");

			var rect = App.Query(c => c.Marked("TheCarouselView")).First().Rect;
			var centerX = rect.CenterX;
			var rightX = rect.X - 5;
			App.DragCoordinates(centerX + 40, rect.CenterY, rightX, rect.CenterY);

			CheckPositionValue("lblPosition", "1");
			CheckPositionValue("lblCurrentItem", "1");
			CheckPositionValue("lblSelected", "1");

			App.Tap(x => x.Marked("btnRemove"));

			CheckPositionValue("lblPosition", "1");
			CheckPositionValue("lblCurrentItem", "2");
			CheckPositionValue("lblSelected", "2");

			App.Back();
		}


		[TestCase("CarouselView (XAML, Horizontal)")]
		public void CarouselViewRemoveFirstCurrentItem(string subgallery)
		{
			VisitSubGallery(subgallery);

			CheckPositionValue("lblPosition", "0");
			CheckPositionValue("lblCurrentItem", "0");
			App.Tap(x => x.Marked("btnRemove"));
			CheckPositionValue("lblPosition", "0");
			CheckPositionValue("lblCurrentItem", "1");
			CheckPositionValue("lblSelected", "1");

			App.Back();
		}


		[TestCase("CarouselView (XAML, Horizontal)")]
		public void CarouselViewRemoveLastCurrentItem(string subgallery)
		{
			VisitSubGallery(subgallery);

			CheckPositionValue("lblPosition", "0");
			CheckPositionValue("lblCurrentItem", "0");
			CheckPositionValue("lblSelected", "0");

			var rect = App.Query(c => c.Marked("TheCarouselView")).First().Rect;
			var centerX = rect.CenterX;
			var rightX = rect.X - 5;
			App.DragCoordinates(centerX + 40, rect.CenterY, rightX, rect.CenterY);
			App.DragCoordinates(centerX + 40, rect.CenterY, rightX, rect.CenterY);
			App.DragCoordinates(centerX + 40, rect.CenterY, rightX, rect.CenterY);
			App.DragCoordinates(centerX + 40, rect.CenterY, rightX, rect.CenterY);
			App.DragCoordinates(centerX + 40, rect.CenterY, rightX, rect.CenterY);

			CheckPositionValue("lblPosition", "4");
			CheckPositionValue("lblCurrentItem", "4");
			CheckPositionValue("lblSelected", "4");

			App.Tap(x => x.Marked("btnRemove"));

			CheckPositionValue("lblPosition", "3");
			CheckPositionValue("lblCurrentItem", "3");
			CheckPositionValue("lblSelected", "3");

			App.Back();
		}

		[TestCase("IndicatorView")]
		public void CarouselViewFirstLastPosition(string subgallery)
		{
			VisitSubGallery(subgallery, true);
			App.WaitForElement("Item: 0");
			App.Tap(x => x.Marked("btnRemoveFirst"));
			App.WaitForElement("Item: 1");
			App.Tap(x => x.Marked("btnNext"));
			App.WaitForElement("Item: 2");
			App.Tap(x => x.Marked("btnRemoveFirst"));
			App.WaitForElement("Item: 2");
			App.Tap(x => x.Marked("btnNext"));
			App.Tap(x => x.Marked("btnNext"));
			App.Tap(x => x.Marked("btnNext"));
			App.Tap(x => x.Marked("btnNext"));
			App.Tap(x => x.Marked("btnNext"));
			App.Tap(x => x.Marked("btnNext"));
			App.Tap(x => x.Marked("btnNext"));
			App.WaitForElement("Item: 9");
			App.Tap(x => x.Marked("btnRemoveLast"));
			App.WaitForElement("Item: 8");
			App.Tap(x => x.Marked("btnPrev"));
			App.WaitForElement("Item: 7");

			App.Back();
		}

		[TestCase("CarouselView (Code, Horizontal)")]
		//[TestCase("CarouselView (XAML, Horizontal)")]
		public void CarouselViewHorizontal(string subgallery)
		{
			VisitSubGallery(subgallery);


			App.WaitForElement("pos:1", "Did start on the correct position");
			var rect = App.Query(c => c.Marked("TheCarouselView")).First().Rect;
			var centerX = rect.CenterX;
			var rightX = rect.X + rect.Width - 1;
			App.DragCoordinates(centerX, rect.CenterY, rightX, rect.CenterY);
			App.WaitForElement("pos:0", "Did not scroll to first position");
			App.DragCoordinates(centerX, rect.CenterY, rect.X + 5, rect.CenterY);
			App.WaitForElement("pos:1", "Did not scroll to second position");

			App.Tap("Item: 1");

			App.WaitForElement("Button works");

			App.Tap(c => c.Marked("Ok"));

			App.Tap("SwipeSwitch");

			// iOS will show the Master page when we try drag
#if __ANDROID__
			App.DragCoordinates(centerX, rect.CenterY, rightX, rect.CenterY);

			App.WaitForNoElement("pos:0", "Swiped while swipe is disabled");
#endif
			App.Back();
		}

#if __IOS__
		[TestCase("CarouselView (Code, Vertical)")]
#endif
		public void CarouselViewVertical(string subgallery)
		{
			VisitSubGallery(subgallery);
			var rect = App.Query(c => c.Marked("TheCarouselView")).First().Rect;

			var centerX = rect.CenterX;
			var centerY = rect.CenterY;
			var bottomY = rect.Y + rect.Height - 1;

			App.DragCoordinates(rect.CenterX, rect.CenterY, rect.CenterX, bottomY);

			App.WaitForElement("pos:0", "Did not scroll to first position");

			App.DragCoordinates(rect.CenterX, rect.CenterY, rect.CenterX, rect.Y - 1);

			App.WaitForElement("pos:1", "Did not scroll to second position");

			App.Tap("Item: 1");

			App.WaitForElement("Button works");

			App.Tap(c => c.Marked("Ok"));

			App.Tap("SwipeSwitch");

#if __ANDROID__
			App.DragCoordinates(rect.CenterX, rect.CenterY, rect.CenterY, rect.Y + rect.Height - 1);

			App.WaitForNoElement("pos:0", "Swiped while swipe is disabled");
#endif
			App.Back();
		}

		void VisitSubGallery(string galleryName, bool enableIndicator = false)
		{
			if (enableIndicator)
				App.Tap(t => t.Marked("EnableIndicatorView"));

			App.QueryUntilPresent(() =>
			{
				App.ScrollDown();
				return App.Query(t => t.Marked(galleryName));
			});

			App.Tap(t => t.Marked(galleryName));
		}

		static void CheckPositionValue(string marked, string value)
		{
			var positionAfter = App.QueryUntilPresent(() =>
			{
				var positionLabel = App.WaitForElement(x => x.Marked(marked));
				if (positionLabel.First().Text == value)
					return positionLabel;
				return null;
			}, delayInMs: 1000);
			Assert.IsTrue(positionAfter[0].Text == value);
		}

	}
}