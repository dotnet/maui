using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue465 : IssuesUITest
	{
		public Issue465(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Change in Navigation.PushModal";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void Issue465TestsPushPopModal()
		{
			RunningApp.WaitForElement("PopPage");
			RunningApp.Screenshot("All elements exist");

			RunningApp.Tap("PopPage");
			RunningApp.WaitForElement("Popppppped");
			RunningApp.Screenshot("Popped modal successful");
		}
	}
}