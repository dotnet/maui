#if TEST_FAILS_ON_CATALYST // App.ScrollUp does nothing: https://github.com/dotnet/maui/issues/31216
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Category(UITestCategories.RefreshView)]
	public class Issue28343 : _IssuesUITest
	{
		public Issue28343(TestDevice testDevice) : base(testDevice)
		{
		}

		protected override bool ResetAfterEachTest => true;

		public override string Issue => "Progress spinner is not disabled after setting content on disabled RefreshView.";

		[Test]
		public void ProgressSpinnerNotDisabledOnStartup()
		{
			App.WaitForElement("RefreshNotTriggered");
			App.WaitForElement("ListItem0");
			App.ScrollUp("CollectionView");
			Thread.Sleep(1000);
			App.WaitForElement("RefreshNotTriggered");
			App.WaitForElement("ScrollToUpButton");
			App.Tap("ScrollToUpButton");
			VerifyScreenshot("Issue28343_ProgressSpinnerDisabled");
		}

		[Test]
		public void ProgressSpinnerRemainsDisabledAfterSwappingContent()
		{
			App.WaitForElement("RefreshNotTriggered");
			App.WaitForElement("ListItem0");
			App.Tap("ResetContent");
			App.WaitForElement("ListItem0");
			App.ScrollUp("CollectionView");
			App.WaitForElement("RefreshNotTriggered");
			VerifyScreenshot("Issue28343_ProgressSpinnerDisabled");
		}

		[Test]
		public void ProgressSpinnerWorksWhenReEnabled()
		{
			App.WaitForElement("SetToEnabled").Tap();
			App.ScrollUp("CollectionView", ScrollStrategy.Gesture, swipePercentage: 0.99, swipeSpeed: 2500);
			App.WaitForElement("RefreshTriggered");
		}
	}
}
#endif
