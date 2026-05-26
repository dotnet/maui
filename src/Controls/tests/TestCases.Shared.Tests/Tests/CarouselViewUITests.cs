#if !WINDOWS
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class CarouselViewUITests : _GalleryUITest
	{
		const string CarouselViewGallery = "CarouselView Gallery";

		public override string GalleryPageName => CarouselViewGallery;

		public CarouselViewUITests(TestDevice device) : base(device)
		{
		}

		protected override bool ResetAfterEachTest => true;

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void CarouselViewSetPosition()
		{

			App.WaitForElement("lblPosition");

			CheckLabelValue("lblPosition", "3");
		}

		[Test]
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

		[Test]
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
		[Test]
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