using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18751 : _IssuesUITest
{
	public Issue18751(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Can scroll CollectionView inside RefreshView";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void Issue18751Test()
	{
		App.WaitForElement("WaitForStubControl");
		// CollectionView uses virtualization which loads images synchronously once items are visible.
		// Use retryTimeout to adaptively wait for any timing variance.
		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
	}
}