using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28051 : _IssuesUITest
{
	public Issue28051(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "ObjectDisposedException Occurs During View Recycling When PlatformBehavior Is Attached on Android";

	const string Refresh = "Refresh";

	[Test]
	[Category(UITestCategories.Performance)]
	public void RefreshItemsShouldNotCrash()
	{
		App.WaitForElement(Refresh);
		for (int i = 0; i < 10; i++)
		{
			App.Tap("RefreshItemsButton");
		}
		App.WaitForElement(Refresh);
	}
}