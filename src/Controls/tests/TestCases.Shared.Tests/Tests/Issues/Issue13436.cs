#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue13436 : _IssuesUITest
	{
		public Issue13436(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Java.Lang.IllegalArgumentException in CarouselView adjusting PeekAreaInsets in OnSizeAllocated using XF 5.0";

		[Test]
		[Category(UITestCategories.CarouselView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroid]
		public void ChangePeekAreaInsetsInOnSizeAllocatedTest()
		{
			App.WaitForElement("CarouselId");
		}
	}
}
#endif