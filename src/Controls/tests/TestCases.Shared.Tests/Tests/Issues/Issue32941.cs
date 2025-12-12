#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // SafeAreaEdges not supported on Catalyst and Windows

using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32941 : _IssuesUITest
{
	public Issue32941(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Label Overlapped by Android Status Bar When Using SafeAreaEdges=Container in .NET MAUI";

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void ShellContentShouldRespectSafeAreaEdges_After_Navigation()
	{
		App.WaitForElement("MainPageLabel");
		App.Tap("GoToSignOutButton");
		App.WaitForElement("SignOutLabel");
		
		// Get the position of the label
		var labelRect = App.FindElement("SignOutLabel").GetRect();
		
		// The label should be positioned below the status bar (Y coordinate should be > 0)
		// On Android with notch, status bar is typically 24-88dp depending on device
		// The label should have adequate top padding from SafeAreaEdges=Container
		Assert.That(labelRect.Y, Is.GreaterThan(0), "Label should not be at Y=0 (would be under status bar)");
		
		// Verify the label is not overlapped by checking it has reasonable top spacing
		// A label at Y < 20 is likely overlapped by the status bar
		Assert.That(labelRect.Y, Is.GreaterThanOrEqualTo(20), 
			"Label Y position should be at least 20 pixels from top to avoid status bar overlap");
	}
}
#endif
