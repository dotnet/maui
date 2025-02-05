#if !WINDOWS
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class CarouselViewUITests : UITest
	{
		const string CarouselViewGallery = "CarouselView Gallery";

		public CarouselViewUITests(TestDevice device)
			: base(device)
		{
		}

		protected override bool ResetAfterEachTest => true;

		public override void TestSetup()
		{
			base.TestSetup();
			App.NavigateToGallery(CarouselViewGallery);
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		public async Task CarouselViewSetPosition()
		{
			await Task.Delay(2000);
			App.WaitForElement("lblPosition");
			await Task.Delay(3000);
			CheckLabelValue("lblPosition", "3");
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		public async Task CarouselViewGoToNextCurrentItem()
		{
			await Task.Delay(2000);
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

		[Test]
		[Category(UITestCategories.CarouselView)]
		public async Task CarouselViewGoToPreviousCurrentItem()
		{
			await Task.Delay(2000);
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
		[Test]
		[Category(UITestCategories.CarouselView)]
		public async Task CarouselViewKeepPositionChangingOrientation()
		{
			await Task.Delay(2000);
			int indexToTest = 3;
			var index = indexToTest.ToString();

			CheckLabelValue("lblPosition", index);
			CheckLabelValue("lblCurrentItem", index);
			App.WaitForElement($"CarouselItem{index}");

			App.SetOrientationLandscape();
			App.SetOrientationPortrait();

			await Task.Delay(3000);

			CheckLabelValue("lblPosition", index);
			CheckLabelValue("lblCurrentItem", index);
		}
#endif

#if IOS || WINDOWS
		[Test]
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

		[Test]
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
			ClassicAssert.AreEqual(value, result);
		}
	}
}
#endif
