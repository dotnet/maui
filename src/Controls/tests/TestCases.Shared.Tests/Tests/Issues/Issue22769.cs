using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22769 : _IssuesUITest
{
	public Issue22769(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Background set to Transparent doesn't have the same behavior as BackgroundColor Transparent";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void ModalPageBackgroundShouldBeTransparent()
	{
		App.WaitForElement("Issue22769Button");
		App.Tap("Issue22769Button");
		App.WaitForElement("Issue22769Label");
		VerifyScreenshot();
	}
}
