# if TEST_FAILS_ON_CATALYST //The ActionSheet fails to close when the user taps outside.
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

		public Issue3049(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "DisplayActionSheet freezes app in iOS custom renderer (iPad only)";

		[Test]
		[Category(UITestCategories.DisplayAlert)]
		[Category(UITestCategories.Compatibility)]
		public async Task Issue3049Test()
		{
			App.WaitForElement(Button1Id);

			App.Tap(Button1Id);

			await Task.Delay(500);
			//App.WaitForElement(Action1);

			// Tap outside ActionSheet to dismiss it
			App.Tap("Click outside ActionSheet instead");

			App.WaitForElement(Button2Id);
			App.Tap(Button2Id);

			App.WaitForElement(Success);
		}
	}
}
#endif