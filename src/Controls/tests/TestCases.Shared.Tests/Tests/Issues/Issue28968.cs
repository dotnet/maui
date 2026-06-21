using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Tests.Issues;

public class Issue28968 : _IssuesUITest
{
	public Issue28968(TestDevice device) : base(device)
	{
	}

	public override string Issue => "[iOS] ActivityIndicator IsRunning ignores IsVisible when set to true";

	[Test]
	[Category(UITestCategories.ActivityIndicator)]
	public void ActivityIndicatorIsRunningDoesNotOverrideIsVisible()
	{
		App.WaitForElement("SetRunningButton");
		App.Tap("SetRunningButton");

		// Wait for the status label to update after the delayed check
		var statusText = App.WaitForElement("StatusLabel").GetText();

		// Retry a few times since the dispatcher delay in the host app
		// means the label won't update immediately
		int retries = 10;
		while (statusText == "Waiting" && retries-- > 0)
		{
			Thread.Sleep(200);
			statusText = App.WaitForElement("StatusLabel").GetText();
		}

		Assert.That(statusText, Is.EqualTo("HIDDEN"),
			"ActivityIndicator should remain hidden when IsVisible=false, even after IsRunning is set to true");
	}
}
