#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS
// The DisplayActionSheet will not dismiss when tapping outside of it, which is a behavior specific to iPads on iOS. Since the host app is running on an iPhone XS, this test can be ignored.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3049 : _IssuesUITest
	{
		const string Button1Id = "button1";
		const string Button2Id = "button2";
		const string Success = "Success";
		const string Skip = "skip";

		public Issue3049(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "DisplayActionSheet freezes app in iOS custom renderer (iPad only)";

		[Test]
		[Category(UITestCategories.DisplayAlert)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest("Skip this test -- as it is not applicable since the host app is not run on iPad in CI")]
		public void Issue3049Test()
		{
			App.WaitForElement(Button1Id);

			var skipLabelRect = App.WaitForElement(Skip).GetRect();

			App.Tap(Button1Id);

			App.WaitForElement("Click outside ActionSheet instead");

			// Tap outside ActionSheet to dismiss it
			App.TapCoordinates(skipLabelRect.CenterX(), skipLabelRect.CenterY());

			App.WaitForElement(Button2Id);
			App.Tap(Button2Id);

			App.WaitForElement(Success);
		}
	}
}
#endif