using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue264 : _IssuesUITest
{
	public Issue264(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "PopModal NRE";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void Issue264TestsPushAndPopModal()
	{
		App.WaitForElement("Home");
		App.WaitForElement("About");

		App.Tap("About");
		App.WaitForElement("CloseMe");

		App.Tap("CloseMe");
		App.WaitForElement("About");

		// Due to the current architecture of the HostApp, we cannot navigate back to the Bug Repro's page.
		// App.Tap("Pop me");
		// App.WaitForElement("Bug Repro's");
	}
}
