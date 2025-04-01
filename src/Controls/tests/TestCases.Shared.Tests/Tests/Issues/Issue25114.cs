using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25114 : _IssuesUITest
{
	public Issue25114(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "NullReferenceException when setting BarBackgroundColor for a NavigationPage";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void NoExceptionShouldBeThrownWhenChangingNavigationBarColor()
	{
		App.WaitForElement("button");
		App.Tap("button");
	}

}