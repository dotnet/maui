# if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS
//This issue affects the DisplayActionSheet feature, where on iPads with a custom renderer, the pop-up menu fails to close when the user taps outside its boundaries.
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
		const string LabelId = "label";

		public Issue3049(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "DisplayActionSheet freezes app in iOS custom renderer (iPad only)";

		[Test]
		[Category(UITestCategories.DisplayAlert)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest("Skip this test -- this is not an iPad, so this is not relevant.")]
		public async Task Issue3049Test()
		{
			App.WaitForElement(Button1Id);

			App.Tap(Button1Id);

			await Task.Delay(500);

			// Tap outside ActionSheet to dismiss it
			App.TapCoordinates(50, 100);

			App.WaitForElement(Button2Id);
			App.Tap(Button2Id);

			App.WaitForElement(Success);
		}
	}
}
#endif