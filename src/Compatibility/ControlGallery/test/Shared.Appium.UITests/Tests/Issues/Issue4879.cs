using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue4879 : IssuesUITest
	{
		public Issue4879(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "4879 - ImageButtonPadding";

		[Test]
		[Category(UITestCategories.ImageButton)]
		public void Issue4879Test()
		{
			App.WaitForElement("TestReady");
			App.Screenshot("I am at Issue 4879 - All buttons/images should be the same size.");
		}
	}
}
