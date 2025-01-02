using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3318 : _IssuesUITest
{
	public Issue3318(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[MAC] ScrollTo method is not working in Xamarin.Forms for mac platform";

	[Test]
	[Category(UITestCategories.ListView)]
	public void Issue3318Test()
	{
		App.WaitForElement("End");
		App.Tap("End");
		App.WaitForElement("Item 19");
	}
}