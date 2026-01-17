using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22802 : _IssuesUITest
{
	public override string Issue => "TemplatedView applies Background to root view";

	public Issue22802(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.ViewBaseTests)]
	public void TemplatedViewBackgroundShouldOnlyApplyToTemplatedElements()
	{
		App.WaitForElement("ChangeBackgroundButton");
		App.Tap("ChangeBackgroundButton");

		VerifyScreenshot();
	}
}
