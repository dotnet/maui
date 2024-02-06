using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	public class CarouselViewUITests : UITest
	{
		const string CarouselViewGallery = "CarouselView Gallery";

		public CarouselViewUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(CarouselViewGallery);
		}

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			this.Back();
		}

		[Test]
		public async Task CarouselViewSetPosition()
		{
			if (Device != TestDevice.Android)
			{
				Assert.Ignore("For now, running this test only on Android.");
			}
			else
			{
				App.WaitForElement("lblPosition");
				await Task.Delay(1000);
				var result = App.FindElement("lblPosition").GetText();
				Assert.AreEqual("3", result);
			}
		}

		[Test]
		public void CarouselViewGoToNextCurrentItem()
		{
			if (Device != TestDevice.Android)
			{
				Assert.Ignore("For now, running this test only on Android.");
			}
			else
			{
				int indexToTest = 3;
				var index = indexToTest.ToString();
				var nextIndex = (indexToTest + 1).ToString();

				CheckLabelValue("lblPosition", index);
				CheckLabelValue("lblCurrentItem", index);

				App.Click("btnNext");
				CheckLabelValue("lblPosition", nextIndex);
				CheckLabelValue("lblCurrentItem", nextIndex);
				CheckLabelValue("lblSelected", nextIndex);
				App.Click("btnPrev");
			}
		}

		void CheckLabelValue(string labelAutomationId, string value)
		{
			var result = App.FindElement(labelAutomationId).GetText();
			Assert.AreEqual(value, result);
		}
	}
}
