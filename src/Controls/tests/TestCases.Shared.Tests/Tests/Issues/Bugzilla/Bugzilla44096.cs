using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla44096 : _IssuesUITest
	{
		public Bugzilla44096(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue =>
			"Grid, StackLayout, and ContentView still participate in hit testing on Android after IsEnabled is set to false";

		/*
		const string StackLayout = "stackLayout";
		const string ContentView = "contentView";
		const string Grid = "grid";
		const string RelativeLayout = "relativeLayout";

		const string Child = "Child";
		const string ToggleColor = "color";
		const string ToggleIsEnabled = "disabled";
		const string Original = "Original";

		[Test]
		[Category(UITestCategories.Layout)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroidWhenRunningOnXamarinUITest]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		public void TestGrid()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			TestControl(Grid);
		}

		[Test]
		[Category(UITestCategories.Layout)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroidWhenRunningOnXamarinUITest]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		public void TestContentView()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			TestControl(ContentView);
		}

		[Test]
		[Category(UITestCategories.Layout)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroidWhenRunningOnXamarinUITest]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		public void TestStackLayout()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			TestControl(StackLayout);
		}


		[Test]
		[Category(UITestCategories.Layout)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest]
		public void TestRelativeLayout()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			TestControl(RelativeLayout);
		}

		void TestControl(string control)
		{
			App.WaitForElement(control);
			App.Tap(control);
			App.WaitForNoElement(Child);

			App.WaitForElement(ToggleColor);
			App.Tap(ToggleColor);

			App.WaitForElement(control);
			App.Tap(control);
			App.WaitForNoElement(Child);

			App.WaitForElement(ToggleIsEnabled);
			App.Tap(ToggleIsEnabled);

			App.WaitForElement(control);
			App.Tap(control);
			App.WaitForNoElement(Original);

			App.WaitForElement(ToggleColor);
			App.Tap(ToggleColor);

			App.WaitForElement(control);
			App.Tap(control);
			App.WaitForNoElement(Original);
		}
		*/
	}
}