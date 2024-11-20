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

	// [Test]
	// [Category(UITestCategories.ListView)]
	// [FailsOnIOS]
	// public void Issue3318Test()
	// {
	// 	App.WaitForElement(q => q.Marked("End"));
	// 	App.Tap(q => q.Marked("End"));
	// 	App.WaitForElement(q => q.Marked("Item 19"));
	// 	App.Back();
	// }
}