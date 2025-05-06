using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20435 : _IssuesUITest
{

	public Issue20435(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "iOS: FlyoutItemImageStyle breaks selection of first item on startup";

	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutItemSelectionVisibleOnStartupWithImageStyle()
	{
		App.WaitForElement("OpenFlyoutButton");
		App.Tap("OpenFlyoutButton");
		VerifyScreenshot();
	}
}