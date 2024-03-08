using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Bugzilla34061 : IssuesUITest
	{
		public Bugzilla34061(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "RelativeLayout - First child added after page display does not appear";

		[Test]
		[Category(UITestCategories.Layout)]
		public void Bugzilla34061Test()
		{
			RunningApp.Screenshot("I am at Bugzilla34061 ");
			RunningApp.WaitForElement("btnAdd");
			RunningApp.Tap("btnAdd");
			RunningApp.WaitForElement("Remove Me");
			RunningApp.Screenshot("I see the button");
		}
	}
}