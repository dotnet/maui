using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla36703 : _IssuesUITest
	{
		const string TestImage = "testimage";
		const string Success = "Success";
		const string Toggle = "toggle";
		const string Testing = "Testing...";

		public Bugzilla36703(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "TapGestureRecognizer inside initially disable Image will never fire Tapped event";

		[Test]
		[Category(UITestCategories.Gestures)]
		public void _36703Test()
		{
			App.WaitForElement(TestImage);
			App.Tap(TestImage);
			App.WaitForElement(Testing);
			App.Tap(Toggle);
			App.Tap(TestImage);
			App.WaitForElement(Success);
		}
	}
}