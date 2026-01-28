using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18896 : _IssuesUITest
{
	const string ListView = "TestListView";

	public Issue18896(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Can scroll ListView inside RefreshView";

	[Test]
	[Category(UITestCategories.ListView)]
	public void Issue18896Test()
	{
		App.WaitForElement("WaitForStubControl");

		App.ScrollDown(ListView);

		App.ScrollUp(ListView);

		// The test passes if you are able to see the image, name, and location of each monkey.
		// Use retryTimeout to wait for images to load and scrollbar to hide
		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
	}
}