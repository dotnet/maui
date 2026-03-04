using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29851 : _IssuesUITest
{

	public Issue29851(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "[iOS] FormattedText with text color causes stack overflow";

	[Test]
	[Category(UITestCategories.Label)]
	public void FormattedTextWithTextColorDoesNotCrash()
	{
		App.WaitForElement("label");
	}
}