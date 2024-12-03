using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla40092 : _IssuesUITest
{
	public Bugzilla40092(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Ensure android devices with fractional scale factors (3.5) don't have a white line around the border";

	// TODO: From Xamarin.UITest migration. 
	// Does some advanced commands to determine layouts, need to find the equivalent on Appium
	// [Test]
	// [Category(UITestCategories.BoxView)]
	// public void AllScreenIsBlack()
	// {
	// 	App.WaitForElement(Ok);
	// 	App.Tap(Ok);
	// 	var box = App.WaitForElement(Black)[0];
	// 	var layout = App.WaitForElement(White)[0];

	// 	var assert = box.Rect.Height == layout.Rect.Height &&
	// 		box.Rect.Width == layout.Rect.Width;

	// 	Assert.IsTrue(assert);
	// }
}