using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla44096 : IssuesUITest
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
		[FailsOnAndroid]
		[FailsOnIOS]
		public void TestGrid()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			TestControl(Grid);
		}

		[Test]
		[Category(UITestCategories.Layout)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void TestContentView()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			TestControl(ContentView);
		}

		[Test]
		[Category(UITestCategories.Layout)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void TestStackLayout()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			TestControl(StackLayout);
		}


		[Test]
		[Category(UITestCategories.Layout)]
		[FailsOnAllPlatforms]
		public void TestRelativeLayout()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			TestControl(RelativeLayout);
		}

		void TestControl(string control)
		{
			RunningApp.WaitForElement(control);
			RunningApp.Tap(control);
			RunningApp.WaitForNoElement(Child);

			RunningApp.WaitForElement(ToggleColor);
			RunningApp.Tap(ToggleColor);

			RunningApp.WaitForElement(control);
			RunningApp.Tap(control);
			RunningApp.WaitForNoElement(Child);

			RunningApp.WaitForElement(ToggleIsEnabled);
			RunningApp.Tap(ToggleIsEnabled);

			RunningApp.WaitForElement(control);
			RunningApp.Tap(control);
			RunningApp.WaitForNoElement(Original);

			RunningApp.WaitForElement(ToggleColor);
			RunningApp.Tap(ToggleColor);

			RunningApp.WaitForElement(control);
			RunningApp.Tap(control);
			RunningApp.WaitForNoElement(Original);
		}
		*/
	}
}