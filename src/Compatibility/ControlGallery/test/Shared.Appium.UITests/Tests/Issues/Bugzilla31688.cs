#if IOS
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla31688 : IssuesUITest
	{
		public Bugzilla31688(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Exception Ancestor must be provided for all pushes except first";
		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.Navigation)]
		[FailsOnAndroid]
		public void Bugzilla31688Test()
		{
			RunningApp.WaitForNoElement("Page3");
		}
	}
}
#endif