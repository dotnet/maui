using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla39636 : IssuesUITest
	{
		public Bugzilla39636(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Cannot use XamlC with OnPlatform in resources, it throws System.InvalidCastException";

		[Test]
		[Category(UITestCategories.LifeCycle)]
		[FailsOnIOS]
		public void DoesNotCrash()
		{ 
			RunningApp.WaitForElement("Success");
		}
	}
}