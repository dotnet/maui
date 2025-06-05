using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21814TabbedPage : _IssuesUITest
{
	public Issue21814TabbedPage(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Add better parameters for navigation args (TabbedPage)";

	[Test]
	[Category(UITestCategories.Navigation)]
	[Category(UITestCategories.TabbedPage)]
	public void VerifyTabbedPageNavigationEventArgs()
	{
		App.WaitForElement("Tab1Content");
	}
}