using NUnit.Framework;
using NUnit.Framework.Legacy;
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

		[Test]
		public void Bugzilla44461Test()
		{
			var positions = TapButton(0);
			Assert.That(positions.initialPosition.X, Is.EqualTo(positions.finalPosition.X));
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
