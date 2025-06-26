using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS
	[Trait("Category", UITestCategories.RefreshView)]
	public class Issue28343 : _IssuesUITest
	{
		public Issue28343(TestDevice testDevice) : base(testDevice)
		{
		}

		protected override bool ResetAfterEachTest => true;

		public override string Issue => "Progress spinner is not disabled after setting content on disabled RefreshView.";

		[Fact]
		public void ProgressSpinnerNotDisabledOnStartup()
		{
			App.WaitForElement("RefreshNotTriggered");
			App.WaitForElement("ListItem0");
			App.ScrollUp("CollectionView");
			App.WaitForElement("RefreshNotTriggered");
			VerifyScreenshot("Issue28343_ProgressSpinnerDisabled");

		}

#if TEST_FAILS_ON_ANDROID // https://github.com/dotnet/maui/issues/28361
		[Fact]
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
#endif

		[Fact]
		public void ProgressSpinnerWorksWhenReEnabled()
		{
			App.WaitForElement("SetToEnabled").Tap();
			App.ScrollUp("CollectionView");
			App.WaitForElement("RefreshTriggered");
		}
	}
#endif
}
