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
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest]
		public void Bugzilla44176Test()
		{
			App.WaitForElement("grid");
			App.Tap("grid");
			App.WaitForNoElement("Parent");

			App.WaitForElement("contentView");
			App.Tap("contentView");
			App.WaitForNoElement("Parent");

			App.WaitForElement("stackLayout");
			App.Tap("stackLayout");
			App.WaitForNoElement("Parent");

			App.WaitForElement("color");
			App.Tap("color");

			App.WaitForElement("grid");
			App.Tap("grid");
			App.WaitForNoElement("Parent");

			App.WaitForElement("contentView");
			App.Tap("contentView");
			App.WaitForNoElement("Parent");

			App.WaitForElement("stackLayout");
			App.Tap("stackLayout");
			App.WaitForNoElement("Parent");

			App.WaitForElement("nontransparent");
			App.Tap("nontransparent");

			App.WaitForElement("grid");
			App.Tap("grid");
			App.WaitForNoElement("Child");

			App.WaitForElement("contentView");
			App.Tap("contentView");
			App.WaitForNoElement("Child");

			App.WaitForElement("stackLayout");
			App.Tap("stackLayout");
			App.WaitForNoElement("Child");

			App.WaitForElement("color");
			App.Tap("color");

			App.WaitForElement("grid");
			App.Tap("grid");
			App.WaitForNoElement("Child");

			App.WaitForElement("contentView");
			App.Tap("contentView");
			App.WaitForNoElement("Child");

			App.WaitForElement("stackLayout");
			App.Tap("stackLayout");
			App.WaitForNoElement("Child");
		}
	}
}