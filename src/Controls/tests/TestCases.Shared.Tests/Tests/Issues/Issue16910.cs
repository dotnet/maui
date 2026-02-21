using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.RefreshView)]
public class Issue16910 : _IssuesUITest
{
	public override string Issue => "IsRefreshing binding works";

	protected override bool ResetAfterEachTest => true;
	public Issue16910(TestDevice device)
		: base(device)
	{

	}

	[Test]
	public void BindingUpdatesFromProgrammaticRefresh()
	{
		App.WaitForElement("RunTest", timeout: TimeSpan.FromSeconds(45));
		App.Tap("RunTest");
		var result = App.WaitForElement("TestResult", timeout: TimeSpan.FromSeconds(45)).GetText();
		Assert.That(result, Is.EqualTo("SUCCESS"));
	}
}