using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;

namespace Xamarin.Forms.Core.UITests
{
	[Category(UITestCategories.CarouselView)]
	[Category(UITestCategories.UwpIgnore)]
	internal class CarouselViewUITests : BaseTestFixture
	{
		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.CarouselViewGallery);
		}

		void SwipeRightToLeft(int swipes = 1)
		{
			var rect = App.Query(c => c.Marked("TheCarouselView")).First().Rect;
			var fromX = rect.CenterX + 40;
			var toX = rect.X - 5;
			var fromY = rect.CenterY;
			var toY = fromY;

			for (int n = 0; n < swipes; n++)
			{
				App.DragCoordinates(fromX, fromY, toX, toY);
			}
		}

		[TestCase("CarouselView (XAML, Horizontal)")]
		[TestCase("CarouselView (XAML, Horizontal, Loop)")]
		public void CarouselViewRemoveAndUpdateCurrentItem(string subgallery)
		{
			VisitSubGallery(subgallery);

			CheckLabelValue("lblPosition", "0");
			CheckLabelValue("lblCurrentItem", "0");
			CheckLabelValue("lblSelected", "0");

			SwipeRightToLeft();

			CheckLabelValue("lblPosition", "1");
			CheckLabelValue("lblCurrentItem", "1");
			CheckLabelValue("lblSelected", "1");

			App.Tap(x => x.Marked("btnRemove"));

			CheckLabelValue("lblPosition", "1");
			CheckLabelValue("lblCurrentItem", "2");
			CheckLabelValue("lblSelected", "2");

			App.Back();
		}

		[TestCase("CarouselView (XAML, Horizontal)")]
		[TestCase("CarouselView (XAML, Horizontal, Loop)")]
		public void CarouselViewRemoveFirstCurrentItem(string subgallery)
		{
			VisitSubGallery(subgallery);

			CheckLabelValue("lblPosition", "0");
			CheckLabelValue("lblCurrentItem", "0");
			App.Tap(x => x.Marked("btnRemove"));
			CheckLabelValue("lblPosition", "0");
			CheckLabelValue("lblCurrentItem", "1");
			CheckLabelValue("lblSelected", "1");

			App.Back();
		}

		[TestCase("CarouselView (XAML, Horizontal)", 0)]
		[TestCase("CarouselView (XAML, Horizontal, Loop)", 0)]
		[TestCase("CarouselView Set CurrentItem", 3)]
		[TestCase("CarouselView Set CurrentItem Loop", 3)]
		public void CarouselViewGoToNextCurrentItem(string subgallery, int indexToTest)
		{
			VisitSubGallery(subgallery);

			var index = indexToTest.ToString();
			var nextIndex = (indexToTest + 1).ToString();

			CheckLabelValue("lblPosition", index);
			CheckLabelValue("lblCurrentItem", index);
			App.Tap(x => x.Marked("btnNext"));
			CheckLabelValue("lblPosition", nextIndex);
			CheckLabelValue("lblCurrentItem", nextIndex);
			CheckLabelValue("lblSelected", nextIndex);
			App.Tap(x => x.Marked("btnPrev"));
			CheckLabelValue("lblPosition", index);
			CheckLabelValue("lblCurrentItem", index);
			CheckLabelValue("lblSelected", index);

			App.Back();
		}

		[TestCase("CarouselView (XAML, Horizontal)")]
		[TestCase("CarouselView (XAML, Horizontal, Loop)")]
		public void CarouselViewRemoveLastCurrentItem(string subgallery)
		{
			VisitSubGallery(subgallery);

			CheckLabelValue("lblPosition", "0");
			CheckLabelValue("lblCurrentItem", "0");
			CheckLabelValue("lblSelected", "0");

			SwipeRightToLeft(4);

			CheckLabelValue("lblPosition", "4");
			CheckLabelValue("lblCurrentItem", "4");
			CheckLabelValue("lblSelected", "4");

			App.Tap(x => x.Marked("btnRemove"));

			CheckLabelValue("lblPosition", "3");
			CheckLabelValue("lblCurrentItem", "3");
			CheckLabelValue("lblSelected", "3");

			App.Back();
		}

		[TestCase("CarouselView (XAML, Horizontal, Loop)")]
		public void CarouselViewLoopAfterLastItem(string subgallery)
		{
			VisitSubGallery(subgallery);

			CheckLabelValue("lblPosition", "0");
			CheckLabelValue("lblCurrentItem", "0");
			CheckLabelValue("lblSelected", "0");

			SwipeRightToLeft(5);

			CheckLabelValue("lblPosition", "0");
			CheckLabelValue("lblCurrentItem", "0");
			CheckLabelValue("lblSelected", "0");

			App.Back();
		}

		[TestCase("CarouselView (XAML, Horizontal, Loop)")]
		public void CarouselViewLoopBeforeFirstItem(string subgallery)
		{
			VisitSubGallery(subgallery);

			CheckLabelValue("lblPosition", "0");
			CheckLabelValue("lblCurrentItem", "0");
			CheckLabelValue("lblSelected", "0");

			var rect = App.Query(c => c.Marked("TheCarouselView")).First().Rect;
			var centerX = rect.CenterX;
			var rightX = rect.X - 5;
			App.DragCoordinates(centerX - 50, rect.CenterY, centerX + rect.Width / 2 - 10, rect.CenterY);

			CheckLabelValue("lblPosition", "4");
			CheckLabelValue("lblCurrentItem", "4");
			CheckLabelValue("lblSelected", "4");

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

		[TestCase("CarouselView SetPosition Ctor")]
		[TestCase("CarouselView SetPosition Appearing")]
		//[TestCase("CarouselView (XAML, Horizontal)")]
		public async System.Threading.Tasks.Task CarouselViewSetPosition(string subgallery)
		{
			VisitSubGallery(subgallery, true);
			App.WaitForElement("lblPosition");
			await System.Threading.Tasks.Task.Delay(1000);
			var result = App.Query(c => c.Marked("lblPosition")).First().Text;
			Assert.AreEqual("3", result);
			App.WaitForElement("4");
			App.Back();
		}

		[TestCase("ObservableCollection and CarouselView")]
		public void CarouselViewObservableCollection(string subgallery)
		{
			VisitSubGallery(subgallery, false);
			App.WaitForElement("lblPosition");
			Assert.AreEqual("0", App.Query(c => c.Marked("lblPosition")).First().Text);
			App.Tap("btnNewObservable");
			Assert.AreEqual("0", App.Query(c => c.Marked("lblPosition")).First().Text);

			SwipeRightToLeft();

			App.Tap("btnAddObservable");
			Assert.AreEqual("0", App.Query(c => c.Marked("lblPosition")).First().Text);

			SwipeRightToLeft();
			Assert.AreEqual("1", App.Query(c => c.Marked("lblPosition")).First().Text);

			SwipeRightToLeft();
			Assert.AreEqual("2", App.Query(c => c.Marked("lblPosition")).First().Text);

			App.Back();
		}

		void VisitSubGallery(string galleryName, bool enableIndicator = false)
		{
			App.ScrollUp();
			App.ScrollUp();

			App.QueryUntilPresent(() =>
			{
				var query = App.Query(t => t.Marked(galleryName));
				if (query.Count() == 0)
				{
					App.ScrollDown();
					return null;
				}
				return query;
			}, delayInMs: 500);

			App.Tap(t => t.Marked(galleryName));
		}

		static void CheckLabelValue(string marked, string value)
		{
			var positionAfter = App.QueryUntilPresent(() =>
			{
				var label = App.WaitForElement(x => x.Marked(marked));
				if (label.First().Text == value)
					return label;
				return null;
			}, delayInMs: 1000);
			Assert.IsTrue(positionAfter[0].Text == value);
		}
	}
}