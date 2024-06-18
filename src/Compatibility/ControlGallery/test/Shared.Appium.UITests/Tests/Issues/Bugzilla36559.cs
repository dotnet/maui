using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla36559 : IssuesUITest
	{
		public Bugzilla36559(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[WP] Navigating to a ContentPage with a Grid inside a TableView affects Entry heights";

		[Test]
		[Category(UITestCategories.Entry)]
		[Category(UITestCategories.TableView)]
		[FailsOnIOS]
		public void Bugzilla36559Test()
		{
			RunningApp.WaitForElement("entry");
			var result = RunningApp.WaitForElement("entry");
			ClassicAssert.AreNotEqual(result.GetRect().Height, -1);
		}
	}
}