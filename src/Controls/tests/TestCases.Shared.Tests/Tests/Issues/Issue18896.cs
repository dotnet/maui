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
	[Ignore("Failing due to image comparison failure, see https://github.com/dotnet/maui/issues/34017")]
	public void Issue18896Test()
	{
		App.WaitForElement("WaitForStubControl");

		App.ScrollDown(ListView);

		App.ScrollUp(ListView);

		// Load images and hide scrollbar.
		// The test passes if you are able to see the image, name, and location of each monkey.
		VerifyScreenshot(retryDelay: TimeSpan.FromSeconds(2));
	}
}