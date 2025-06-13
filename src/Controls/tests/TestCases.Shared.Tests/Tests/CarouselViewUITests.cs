#if !WINDOWS
using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class CarouselViewUITests : UITest
	{
		const string CarouselViewGallery = "CarouselView Gallery";

		public CarouselViewUITests(TestDevice device) : base(device)
		{
		}

		protected override bool ResetAfterEachTest => true;

		public override void TestSetup()
		{
			base.TestSetup();
			App.NavigateToGallery(CarouselViewGallery);
		}

		[Fact]
		[Category(UITestCategories.CarouselView)]
		public void CarouselViewSetPosition()
		{

			App.WaitForElement("lblPosition");

			CheckLabelValue("lblPosition", "3");
		}

		[Fact]
		[Category(UITestCategories.CarouselView)]
		public void CarouselViewGoToNextCurrentItem()
		{
			int indexToTest = 3;
			var index = indexToTest.ToString();
			var nextIndex = (indexToTest + 1).ToString();

			CheckLabelValue("lblPosition", index);
			CheckLabelValue("lblCurrentItem", index);
			App.WaitForElement($"CarouselItem{index}");

			App.Tap("btnNext");
			CheckLabelValue("lblPosition", nextIndex);
			CheckLabelValue("lblCurrentItem", nextIndex);
			CheckLabelValue("lblSelected", nextIndex);
			App.Tap("btnPrev");
		}

		[Fact]
		[Category(UITestCategories.CarouselView)]
		public void CarouselViewGoToPreviousCurrentItem()
		{
			int indexToTest = 3;
			var index = indexToTest.ToString();
			var previousIndex = (indexToTest - 1).ToString();

			CheckLabelValue("lblPosition", index);
			CheckLabelValue("lblCurrentItem", index);
			App.WaitForElement($"CarouselItem{index}");

			App.Tap("btnPrev");
			CheckLabelValue("lblPosition", previousIndex);
			CheckLabelValue("lblCurrentItem", previousIndex);
			CheckLabelValue("lblSelected", previousIndex);
		}

		// Catalyst doesn't support orientation changes
#if !MACCATALYST
		[Fact]
		[Category(UITestCategories.CarouselView)]
		public void CarouselViewKeepPositionChangingOrientation()
		{

			int indexToTest = 3;
			var index = indexToTest.ToString();

			CheckLabelValue("lblPosition", index);
			CheckLabelValue("lblCurrentItem", index);
			App.WaitForElement($"CarouselItem{index}");

			App.SetOrientationLandscape();
			App.WaitForElement($"CarouselItem{index}");
			App.SetOrientationPortrait();
			App.WaitForElement($"CarouselItem{index}");



			CheckLabelValue("lblPosition", index);
			CheckLabelValue("lblCurrentItem", index);
		}
#endif

#if IOS || WINDOWS
		[Fact]
		[Category(UITestCategories.CarouselView)]
		public void NavigateBackWhenLooped()
		{
			int index = 3;

			for (int i = 0; i < 10; i++)
			{
				if (index < 0)
				{
					index = 4;
				}

				App.WaitForElement($"CarouselItem{index}");
				App.ScrollLeft("TheCarouselView");
				index--;
			}
		}

		[Fact]
		[Category(UITestCategories.CarouselView)]
		public void NavigateForwardWhenLooped()
		{
			int index = 3;

			for (int i = 0; i < 10; i++)
			{
				if (index > 4)
				{
					index = 0;
				}

				App.WaitForElement($"CarouselItem{index}");
				App.ScrollRight("TheCarouselView");
				index++;
			}
		}
#endif

		void CheckLabelValue(string labelAutomationId, string value)
		{
			var result = App.FindElement(labelAutomationId).GetText();
			Assert.Equal(value, result);
		}
	}
}
#endif