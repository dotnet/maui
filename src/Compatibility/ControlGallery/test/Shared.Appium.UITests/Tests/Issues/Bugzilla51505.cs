using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla51505 : IssuesUITest
	{
		const string ButtonId = "button";

		public Bugzilla51505(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ObjectDisposedException On Effect detachment.";

		[Test]
		[Category(UITestCategories.Button)]
		[Category(UITestCategories.Effects)]
		public void Bugzilla51505Test()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(ButtonId);
			Assert.DoesNotThrow(() => RunningApp.Tap(ButtonId));
		}
	}
}