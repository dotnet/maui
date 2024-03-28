using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla37625 : IssuesUITest
	{
		public Bugzilla37625(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "App crashes when quickly adding/removing Image views (Windows UWP)";

		[Test]
		[Category(UITestCategories.Image)]
		public void Bugzilla37625Test()
		{
			RunningApp.WaitForElement("success");
		}
	}
}