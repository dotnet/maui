using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla39821 : IssuesUITest
	{
		public Bugzilla39821(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ViewExtension.TranslateTo cannot be invoked on Main thread";

		[Test]
		[Category(UITestCategories.Animation)]
		[Ignore("Fails intermittently")]
		public void DoesNotCrash()
		{
			RunningApp.Tap("Animate");
			RunningApp.WaitForNoElement("Success", timeout: TimeSpan.FromSeconds(25));
		}
	}
}