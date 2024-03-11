using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla46458 : IssuesUITest
	{
		public Bugzilla46458(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Grid.IsEnabled property is not working";

		[Test]
		[Category(UITestCategories.Layout)]
		public void GridIsEnabled()
		{
			RunningApp.WaitForElement("entry");
			RunningApp.Tap("entry");
			RunningApp.WaitForNoElement("Success");

			RunningApp.WaitForElement("button");
			RunningApp.Tap("button");
			RunningApp.WaitForNoElement("Success");

			RunningApp.WaitForElement("button1");
			RunningApp.Tap("button1");
			RunningApp.WaitForElement("Clicked");

			RunningApp.WaitForElement("entry");
			RunningApp.Tap("entry");
			RunningApp.WaitForNoElement("Success");

			RunningApp.WaitForElement("button");
			RunningApp.Tap("button");
			RunningApp.WaitForNoElement("Success");
		}
	}
}