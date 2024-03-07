using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue1267 : IssuesUITest
	{
		const string Success = "If this is visible, the test has passed.";

		public Issue1267(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Star '*' in Grid layout throws exception";

		[Test]
		[Category(UITestCategories.Layout)]
		public void StarInGridDoesNotCrash()
		{
			RunningApp.WaitForNoElement(Success);
		}
	}
}