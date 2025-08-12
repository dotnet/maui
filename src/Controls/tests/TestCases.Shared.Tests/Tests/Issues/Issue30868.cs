using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30868 : _IssuesUITest
{
	public Issue30868(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "CollectionView selection visual states";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewSelectionMode()
	{
		App.WaitForElement("Item 2");
		App.Tap("Item 2");
		VerifyScreenshot();
	}
}