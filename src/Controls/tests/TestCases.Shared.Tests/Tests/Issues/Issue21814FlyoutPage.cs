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
	public void VerifyFlyoutPageInitialNavigationEventArgs()
	{
		App.WaitForElement("FlyoutContent1");
		
		App.WaitForElement("FlyoutItem1OnNavigatedToLabel");
		var navigatedTo = App.FindElement("FlyoutItem1OnNavigatedToLabel").GetText();
		Assert.That(navigatedTo, Is.EqualTo("PreviousPage: Null, NavigationType: Push"));
	}
}