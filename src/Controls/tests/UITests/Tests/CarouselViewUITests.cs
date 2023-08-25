using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests
{
	public class CarouselViewUITests : UITestBase
	{
		const string CarouselViewGallery = "* marked:'CarouselView Gallery'";

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
			App.NavigateBack();
		}

		//[Test]
		//public async Task CarouselViewSetPosition()
		//{
		//	if (Device != TestDevice.Android)
		//	{
		//		Assert.Ignore("For now, running this test only on Android.");
		//	}
		//	else
		//	{
		//		App.WaitForElement("lblPosition");
		//		await Task.Delay(1000);
		//		var result = App.Query(c => c.Marked("lblPosition")).First().Text;
		//		Assert.AreEqual("3", result);
		//	}
		//}

		//[Test]
		//public void CarouselViewGoToNextCurrentItem()
		//{
		//	if (Device != TestDevice.Android)
		//	{
		//		Assert.Ignore("For now, running this test only on Android.");
		//	}
		//	else
		//	{
		//		int indexToTest = 3;
		//		var index = indexToTest.ToString();
		//		var nextIndex = (indexToTest + 1).ToString();

		//		CheckLabelValue("lblPosition", index);
		//		CheckLabelValue("lblCurrentItem", index);

		//		App.Tap(x => x.Marked("btnNext"));
		//		CheckLabelValue("lblPosition", nextIndex);
		//		CheckLabelValue("lblCurrentItem", nextIndex);
		//		CheckLabelValue("lblSelected", nextIndex);
		//	}
		//}

		static void CheckLabelValue(string labelAutomationId, string value)
		{
			var result = App.Query(c => c.Marked(labelAutomationId)).First().Text;
			Assert.AreEqual(value, result);
		}
	}
}
