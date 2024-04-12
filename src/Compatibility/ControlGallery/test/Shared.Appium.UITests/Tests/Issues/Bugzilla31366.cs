using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla31366 : IssuesUITest
	{
		public Bugzilla31366(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Pushing and then popping a page modally causes ArgumentOutOfRangeException";

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			RunningApp.Back();
		}

		[Test]
		[Category(UITestCategories.Navigation)]
		[FailsOnIOS]
		public void Issue31366PushingAndPoppingModallyCausesArgumentOutOfRangeException()
		{
			RunningApp.Tap("StartPopOnAppearingTest");
			RunningApp.WaitForNoElement("If this is visible, the PopOnAppearing test has passed.");
		}

		[Test]
		[Category(UITestCategories.Navigation)]
		[FailsOnIOS]
		public void Issue31366PushingWithModalStackCausesIncorrectStackOrder()
		{
			RunningApp.Tap("StartModalStackTest");
			RunningApp.WaitForNoElement("If this is visible, the modal stack test has passed.");
			RunningApp.Back();
		}
	}
}