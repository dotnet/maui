#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS
// Pull-to-refresh via Appium gesture is only supported on iOS and Android

using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33169 : _IssuesUITest
{
	public Issue33169(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] RefreshView with CollectionView shows graphical glitches when Large Page Titles are enabled";

	[Test]
	[Category(UITestCategories.RefreshView)]
	public void RefreshViewWorksWithLargeTitles()
	{
		App.WaitForElement("TestRefreshView");

		// Use a real pull gesture to trigger UIRefreshControl.ValueChanged
		App.ScrollUp("TestRefreshView");

		// The Command should fire and set StatusLabel to "SUCCESS"
		App.WaitForTextToBePresentInElement("StatusLabel", "SUCCESS");
	}
}

#endif
