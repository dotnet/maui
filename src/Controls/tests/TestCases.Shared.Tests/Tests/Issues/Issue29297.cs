#if TEST_FAILS_ON_ANDROID // Loaded event is not triggered when navigating back to previous page - https://github.com/dotnet/maui/issues/29414
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue29297 : _IssuesUITest
{
	public Issue29297(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Crash on shell navigation for Mac Catalyst";

	[Test]
	[Category(UITestCategories.Shell)]
	public void Shell_Issue29297()
	{
		App.WaitForElement("Button");
		App.Click("Button");
		App.WaitForElement("Button");
		App.Click("Button");
		App.WaitForElement("Successfully navigated back");
	}
}
#endif