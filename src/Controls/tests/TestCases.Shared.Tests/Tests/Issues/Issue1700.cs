using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1700 : _IssuesUITest
{
	const string Success = "Success";

	public Issue1700(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Image fails loading from long URL";


	[Test]
	[Category(UITestCategories.Image)]
	[Category(UITestCategories.Compatibility)]
	public void LongImageURLsShouldNotCrash()
	{
		// Give the images some time to load (or fail)
		Task.Delay(3000).Wait();
		App.WaitForElement("Legumes");
		App.WaitForElement("Vegetables");
		// If we can see this label at all, it means we didn't crash and the test is successful
		App.WaitForElement(Success);
	}
}