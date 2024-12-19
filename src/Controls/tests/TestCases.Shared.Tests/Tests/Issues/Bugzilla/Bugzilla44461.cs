#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS//position value not updating
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Category(UITestCategories.ScrollView)]
	[Category(UITestCategories.Compatibility)]
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
			var positions = TapButton(0);
			Assert.That(positions.initialPosition.X, Is.EqualTo(positions.finalPosition.X));
			Assert.That(positions.finalPosition.X, Is.LessThanOrEqualTo(1));

			App.Screenshot("Button0 is aligned with the left side of the screen");
		}

		(System.Drawing.Rectangle initialPosition, System.Drawing.Rectangle finalPosition) TapButton(int position)
		{
			var buttonId = $"{position}";
			App.WaitForElement(buttonId);
			var initialPosition = App.FindElement(buttonId).GetRect();
			App.Tap(buttonId);
			var finalPosition = App.FindElement(buttonId).GetRect();
			return (initialPosition, finalPosition);
		}
	}
}
#endif