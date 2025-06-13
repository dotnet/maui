#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST   //When clicking Button0 (the first button) incorrectly centers it in the ScrollView instead of maintaining its left alignment.                                                                                                                                 
//Issue Link: https://github.com/dotnet/maui/issues/26760
using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Category(UITestCategories.ScrollView)]
	public class Bugzilla44461UITests : _IssuesUITest
	{
		public Bugzilla44461UITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "ScrollToPosition.Center works differently on Android and iOS";

		[Fact]
		[FailsOnIOSWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		[FailsOnMacWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		public void Bugzilla44461Test()
		{
			var positions = TapButton(0);
			Assert.Equal(positions.finalPosition.X, positions.initialPosition.X);
			Assert.That(positions.finalPosition.X, Is.LessThanOrEqualTo(1));
		}

		(System.Drawing.Rectangle initialPosition, System.Drawing.Rectangle finalPosition) TapButton(int position)
		{
			var buttonId = $"{position}";
			App.WaitForElement(buttonId);
			var initialPosition = App.WaitForElement(buttonId).GetRect();
			App.Tap(buttonId);
			var finalPosition = App.WaitForElement(buttonId).GetRect();
			return (initialPosition, finalPosition);
		}
	}
}
#endif