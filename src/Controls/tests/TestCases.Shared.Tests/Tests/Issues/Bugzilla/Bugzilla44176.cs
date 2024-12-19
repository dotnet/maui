#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST &&TEST_FAILS_ON_WINDOWS //Tap not working 
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla44176 : _IssuesUITest
	{
		public Bugzilla44176(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "InputTransparent fails if BackgroundColor not explicitly set on Android";

		[Test]
		[Category(UITestCategories.Layout)]
		[Category(UITestCategories.Compatibility)]
		public void Bugzilla44176Test()
		{
			App.WaitForElement("grid");
			App.Tap("grid");
			App.WaitForElement("Parent");

			App.WaitForElement("contentView");
			App.Tap("contentView");
			App.WaitForElement("Parent");

			App.WaitForElement("stackLayout");
			App.Tap("stackLayout");
			App.WaitForElement("Parent");

			App.WaitForElement("color");
			App.Tap("color");

			App.WaitForElement("grid");
			App.Tap("grid");
			App.WaitForElement("Parent");

			App.WaitForElement("contentView");
			App.Tap("contentView");
			App.WaitForElement("Parent");

			App.WaitForElement("stackLayout");
			App.Tap("stackLayout");
			App.WaitForElement("Parent");

			App.WaitForElement("nontransparent");
			App.Tap("nontransparent");

			App.WaitForElement("grid");
			App.Tap("grid");
			App.WaitForElement("Child");

			App.WaitForElement("contentView");
			App.Tap("contentView");
			App.WaitForElement("Child");

			App.WaitForElement("stackLayout");
			App.Tap("stackLayout");
			App.WaitForElement("Child");

			App.WaitForElement("color");
			App.Tap("color");

			App.WaitForElement("grid");
			App.Tap("grid");
			App.WaitForElement("Child");

			App.WaitForElement("contentView");
			App.Tap("contentView");
			App.WaitForElement("Child");

			App.WaitForElement("stackLayout");
			App.Tap("stackLayout");
			App.WaitForElement("Child");
		}
	}
}
#endif