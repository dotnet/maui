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

		[Test]
		[Category(UITestCategories.Navigation)]
		public void Bugzilla31688Test()
		{
			RunningApp.WaitForNoElement("Page3");
		}
	}
}