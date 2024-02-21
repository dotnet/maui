//using Microsoft.Maui.Appium;
//using NUnit.Framework;
//using Xamarin.UITest.Queries;

//namespace Microsoft.Maui.AppiumTests.Issues
//{
//	public class Issue12567 : _IssuesUITest
//	{
//		public Issue12567(TestDevice device) : base(device) { }

//		public override string Issue => "Carousel View does not behave properly in Windows";

//		[Test]
//		public void WhenQueryingCarouselItemsInViewThenSingleItemIsRetrieved()
//		{
//			// TODO: Investigate and remove the need for this. Currently all platforms fail the "Assert.AreEqual(10, itemNumber)",
//			// though failures are a bit different on different platforms.
//			Assert.Ignore("WhenQueryingCarouselItemsInViewThenSingleItemIsRetrieved is newly added but fails; need to investigate and fix");

//			//Assert the initial item is the only one displayed
//			AssertSingleCarouselItem();

//			//Tap the button so it goes to the last item in the carousel
//			App.Tap("GetLast");

//			//Assert the last item is the only one displayed
//			AssertSingleCarouselItem();

//			var itemNumber = GetCurrentItemNumber();

//			Assert.AreEqual(10, itemNumber);
//		}

//		int GetCurrentItemNumber()
//		{
//			var numberLabel = App.Query(element => element.Marked("NumberLabel")).Where(IsActiveCarouselItem).First();

//			return int.Parse(numberLabel.Text);
//		}

//		void AssertSingleCarouselItem()
//		{
//			// Get the visible labels in the carousel (should be one if everything is OK)
//			var labels = App.FindElement(element => element.Marked("NameLabel")).Where(IsActiveCarouselItem);

//			Assert.NotNull(labels);
//			Assert.IsNotEmpty(labels);
//			//Only one item should be visible at a time in the carousel
//			Assert.AreEqual(1, labels.Count());
//		}

//		//Only the active item in the window should have a size greater than zero
//		bool IsActiveCarouselItem(AppResult carouselItem) => carouselItem.Rect.Width > 0 && carouselItem.Rect.Height > 0;
//	}
//}