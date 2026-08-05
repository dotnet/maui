#if IOS || MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue99999 : _IssuesUITest
{
	public Issue99999(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Shell flyout footer unsubscribes MeasureInvalidated when replaced";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ReplacedFooterDoesNotInvalidateCurrentFooterMeasure()
	{
		App.WaitForElement("PrepareFooters");
		App.Tap("PrepareFooters");
		App.WaitForTextToBePresentInElement("FooterMeasureStatus", "Ready");

		App.Tap("InvalidateOldFooter");

		Assert.That(
			App.WaitForElement("FooterMeasureStatus").GetText(),
			Is.EqualTo("Current footer measure count: 0"));
	}
}
#endif