using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	[Category(UITestCategories.ScrollView)]
	public class Bugzilla44461UITests : _IssuesUITest
	{
		public Bugzilla44461UITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "ScrollToPosition.Center works differently on Android and iOS";

		// Bugzilla44461 (src\Compatibility\ControlGallery\src\Issues.Shared\Bugzilla44461.cs)
		[Test]
		public void Bugzilla44461Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac, TestDevice.Windows },
				"This test is failing, likely due to product issue");

			var positions = TapButton(0);
			ClassicAssert.AreEqual(positions.initialPosition.X, positions.finalPosition.X);
			ClassicAssert.LessOrEqual(positions.finalPosition.X, 1);
			App.Screenshot("Button0 is aligned with the left side of the screen");
		}

		(System.Drawing.Rectangle initialPosition, System.Drawing.Rectangle finalPosition) TapButton(int position)
		{
			var buttonId= $"{position}";
			App.WaitForElement(buttonId);
			var initialPosition = App.FindElement(buttonId).GetRect();
			App.Tap(buttonId);
			var finalPosition = App.FindElement(buttonId).GetRect();
			return (initialPosition, finalPosition);
		}
	}
}