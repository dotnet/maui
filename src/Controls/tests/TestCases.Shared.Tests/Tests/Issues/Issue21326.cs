using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21326 : _IssuesUITest
{
	public override string Issue => "Span does not inherit text styling from Label if that styling is applied using Style";

	public Issue21326(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Label)]
	public void SpanShouldInheritStyleFromLabel()
	{
		App.WaitForElement("Issue21326Label");
		VerifyScreenshot();
	}
}
