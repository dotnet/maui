namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35492 : _IssuesUITest
{
	public override string Issue => "Border.StrokeDashArray leaks dashed Borders when using a shared Application resource";

	public Issue35492(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Border)]
	public void DashboardShowsSingleRunLeakActionAndSandboxText()
	{
		App.WaitForElement("RunSharedResourceLeakButton");
		App.Tap("RunSharedResourceLeakButton");
		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
	}
}
