using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

internal class Issue11905 : _IssuesUITest
{
	public Issue11905(TestDevice device) : base(device) { }

	public override string Issue => "WebView.EvaluateJavaScriptAsync fails to execute JavaScript containing newline characters";

	[Test]
	[Category(UITestCategories.WebView)]
	public void JavaScriptWithNewlinesShouldExecuteCorrectly()
	{
		App.WaitForElement("TestLabel");
	}
}