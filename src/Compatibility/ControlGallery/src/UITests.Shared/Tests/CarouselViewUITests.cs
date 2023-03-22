using System;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;

namespace Microsoft.Maui.Controls.Compatibility.UITests
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

			try
			{
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
			}
			finally
			{
				App.Back();
			}
		}

		[TestCase("CarouselView (XAML, Horizontal)")]
		[TestCase("CarouselView (XAML, Horizontal, Loop)")]
		public void CarouselViewRemoveFirstCurrentItem(string subgallery)
		{
			VisitSubGallery(subgallery);

			try
			{
				CheckLabelValue("lblPosition", "0");
				CheckLabelValue("lblCurrentItem", "0");
				App.Tap(x => x.Marked("btnRemove"));
				CheckLabelValue("lblPosition", "0");
				CheckLabelValue("lblCurrentItem", "1");
				CheckLabelValue("lblSelected", "1");
			}
			finally
			{
				App.Back();
			}
		}

		[TestCase("CarouselView (XAML, Horizontal)", 0)]
		[TestCase("CarouselView (XAML, Horizontal, Loop)", 0)]
		[TestCase("CarouselView Set CurrentItem", 3)]
		[TestCase("CarouselView Set CurrentItem Loop", 3)]
		public void CarouselViewGoToNextCurrentItem(string subgallery, int indexToTest)
		{
			VisitSubGallery(subgallery);
			try
			{
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
			}
			finally
			{
				App.Back();
			}
		}

		[TestCase("CarouselView (XAML, Horizontal)")]
		[TestCase("CarouselView (XAML, Horizontal, Loop)")]
		public void CarouselViewRemoveLastCurrentItem(string subgallery)
		{
			VisitSubGallery(subgallery);

			try
			{
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
			}
			finally
			{
				App.Back();
			}
		}

		[TestCase("CarouselView (XAML, Horizontal, Loop)")]
		public void CarouselViewLoopAfterLastItem(string subgallery)
		{
			VisitSubGallery(subgallery);

			try
			{
				CheckLabelValue("lblPosition", "0");
				CheckLabelValue("lblCurrentItem", "0");
				CheckLabelValue("lblSelected", "0");

				SwipeRightToLeft(5);

				CheckLabelValue("lblPosition", "0");
				CheckLabelValue("lblCurrentItem", "0");
				CheckLabelValue("lblSelected", "0");
			}
			finally
			{
				App.Back();
			}
		}

		[TestCase("CarouselView (XAML, Horizontal, Loop)")]
		public void CarouselViewLoopBeforeFirstItem(string subgallery)
		{
			VisitSubGallery(subgallery);

			try
			{
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
			}
			finally
			{
				App.Back();
			}
		}

		[TestCase("IndicatorView")]
		public void CarouselViewFirstLastPosition(string subgallery)
		{
			VisitSubGallery(subgallery, true);

			try
			{
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
			}
			finally
			{
				App.Back();
			}
		}

		[TestCase("CarouselView (Code, Horizontal)")]
		//[TestCase("CarouselView (XAML, Horizontal)")]
		public void CarouselViewHorizontal(string subgallery)
		{
			VisitSubGallery(subgallery);

			try
			{
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
			}
			finally
			{
				App.Back();
			}
		}

#if __IOS__
		[TestCase("CarouselView (Code, Vertical)")]
#endif
		public void CarouselViewVertical(string subgallery)
		{
			VisitSubGallery(subgallery);

			try
			{
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
			}
			finally
			{
				App.Back();
			}
		}

		[TestCase("CarouselView SetPosition Ctor")]
		[TestCase("CarouselView SetPosition Appearing")]
		//[TestCase("CarouselView (XAML, Horizontal)")]
		public async System.Threading.Tasks.Task CarouselViewSetPosition(string subgallery)
		{
			VisitSubGallery(subgallery, true);

			try
			{
				App.WaitForElement("lblPosition");
				await System.Threading.Tasks.Task.Delay(1000);
				var result = App.Query(c => c.Marked("lblPosition")).First().Text;
				Assert.AreEqual("3", result);
				App.WaitForElement("4");
			}
			finally
			{
				App.Back();
			}
		}

		[TestCase("ObservableCollection and CarouselView")]
		public void CarouselViewObservableCollection(string subgallery)
		{
			VisitSubGallery(subgallery, false);

			try
			{
				App.WaitForElement("lblPosition");

				string GetPosition()
				{
					return App.Query(c => c.Marked("lblPosition")).First().Text;
				}

				Assert.AreEqual("0", GetPosition());
				App.Tap("btnNewObservable");
				Assert.AreEqual("0", GetPosition());

				SwipeRightToLeft();

				App.Tap("btnAddObservable");
				Assert.AreEqual("0", GetPosition());

				SwipeRightToLeft();
				Assert.AreEqual("1", GetPosition());

				SwipeRightToLeft();
				Assert.AreEqual("2", GetPosition());
			}
			finally
			{
				App.Back();
			}
		}

		void VisitSubGallery(string galleryName, bool enableIndicator = false)
		{
			if (App.Query("CarouselView Galleries").Length == 0)
			{
				App.ScrollUpTo("CarouselView Galleries", timeout: TimeSpan.FromSeconds(60));
			}

			if (App.Query(galleryName).Length == 0)
			{
				App.ScrollDownTo(galleryName, timeout: TimeSpan.FromSeconds(60));
			}

			App.Tap(galleryName);
		}

		static void CheckLabelValue(string marked, string value)
		{
			var positionAfter = App.QueryUntilPresent(() =>
			{
				var label = App.WaitForElement(x => x.Marked(marked), timeout: TimeSpan.FromSeconds(10));
				if (label.First().Text == value)
					return label;
				return null;
			}, delayInMs: 1000);
			Assert.IsTrue(positionAfter[0].Text == value);
		}
	}
}