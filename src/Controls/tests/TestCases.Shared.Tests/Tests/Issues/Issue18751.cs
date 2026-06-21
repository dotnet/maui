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
		// Load images and hide scrollbar.
		// The test passes if you are able to see the image, name, and location of each monkey.
		VerifyScreenshot(retryDelay: TimeSpan.FromSeconds(2));
	}
}