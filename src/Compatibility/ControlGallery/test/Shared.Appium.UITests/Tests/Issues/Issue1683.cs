using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue1683 : IssuesUITest
	{
		public Issue1683(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Auto Capitalization Implementation";

		[Test]
		[Category(UITestCategories.Entry)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void Issue1683Test()
		{
			RunningApp.WaitForElement("Rotation");

			for (int i = 0; i < 6; i++)
			{
				RunningApp.Tap("Rotation");
			}
		}
	}
}