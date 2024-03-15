using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue10222 : IssuesUITest
	{
		public Issue10222(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Exception Ancestor must be provided for all pushes except first";

		[Test]
		[Category(UITestCategories.LifeCycle)]
		public void Issue10222Test()
		{
			RunningApp.WaitForElement("goTo");
			RunningApp.Tap("goTo");
			RunningApp.WaitForElement("collectionView");
			RunningApp.WaitForElement("goTo");
		}
	}
}