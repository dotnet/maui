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

		[Test]
		[Category(UITestCategories.CarouselView)]
		public async Task CarouselViewSetPosition()
		{
			App.NavigateToGallery(CarouselViewGallery);
			await Task.Delay(2000);
			try
			{
				App.WaitForElement("lblPosition");
				await Task.Delay(3000);
				CheckLabelValue("lblPosition", "3");
			}
			catch
			{
				App.Screenshot("Failed to Start on the correct position ");
			}
			finally
			{
				Reset();
			}
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void CarouselViewGoToNextCurrentItem()
		{
			App.NavigateToGallery(CarouselViewGallery);
			try
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
			catch
			{
				App.Screenshot("Failed to tap on btnNext");
			}
			finally
			{
				Reset();
			}
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void CarouselViewGoToPreviousCurrentItem()
		{
			App.NavigateToGallery(CarouselViewGallery);
			try
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
			catch
			{
				App.Screenshot("Failed to tap on btnPrev");
			}
			finally
			{
				Reset();
			}
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		public async Task CarouselViewKeepPositionChangingOrientation()
		{
			App.NavigateToGallery(CarouselViewGallery);
			try
			{
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
			catch
			{
				App.Screenshot("Failed to tap on btnSetPosition");
			}
			finally
			{
				Reset();
			}
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void NavigateBackWhenLooped()
		{
			App.NavigateToGallery(CarouselViewGallery);
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
			App.NavigateToGallery(CarouselViewGallery);
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

		void CheckLabelValue(string labelAutomationId, string value)
		{
			var result = App.FindElement(labelAutomationId).GetText();
			ClassicAssert.AreEqual(value, result);
		}
	}
}
#endif