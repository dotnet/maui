using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue13616 : _IssuesUITest
{
	public Issue13616(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] After updating XF 5.0.0.1931 getting Java.Lang.IllegalArgumentException: Invalid target position at Java.Interop.JniEnvironment+InstanceMethods.CallVoidMethod";

	[Test]
	[Category(UITestCategories.CarouselView)]
	[Category(UITestCategories.Compatibility)]
	public void Issue13616Test()
	{
		App.WaitForElement("AddItemButtonId");
		App.Tap("AddItemButtonId");
		App.WaitForElement("CarouselViewId");
	}
}