using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21814FlyoutPage : _IssuesUITest
{
	public Issue21814FlyoutPage(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Add better parameters for navigation args (FlyoutPage)";

	[Test]
	[Category(UITestCategories.Navigation)]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutPageNavigationEventArgs()
	{
		App.WaitForElement("FlyoutContent1");
	}
}