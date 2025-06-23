using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7339 : _IssuesUITest
{
	public Issue7339(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] Material frame renderer not being cleared";


	[Test]
	[Category(UITestCategories.Shell)]
	public void MaterialFrameDisposesCorrectly()
	{
		App.WaitForElement("InstructionLabel");
		App.TapInShellFlyout("Item1");
		App.WaitForElementTillPageNavigationSettled("InstructionLabel");
		App.TapInShellFlyout("Item2");
		App.WaitForElementTillPageNavigationSettled("FrameContent");
		App.TapInShellFlyout("Item1");
		App.WaitForElementTillPageNavigationSettled("InstructionLabel");
		App.TapInShellFlyout("Item2");
		App.WaitForElementTillPageNavigationSettled("FrameContent");

	}
}