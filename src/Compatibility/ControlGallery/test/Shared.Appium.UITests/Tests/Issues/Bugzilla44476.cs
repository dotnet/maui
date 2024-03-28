using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla44476 : IssuesUITest
	{
		public Bugzilla44476(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Exception Ancestor must be provided for all pushes except first";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void Issue44476TestUnwantedMargin()
		{
			RunningApp.WaitForNoElement("This should be visible.");
		}
	}
}