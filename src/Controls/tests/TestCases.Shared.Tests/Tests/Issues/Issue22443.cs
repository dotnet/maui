using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22443 : _IssuesUITest
{
	public Issue22443(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "App Crash on Scroll Animation while navigating away from Page";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void NoExceptionShouldBeThrown()
	{
		App.WaitForElement("button");
		App.Click("button");
		App.WaitForElement("button");

		//The test passes if no exception is thrown
	}
}
