using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla31366 : _IssuesUITest
	{
		public Bugzilla31366(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Pushing and then popping a page modally causes ArgumentOutOfRangeException";

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOS]
		public void Issue31366PushingAndPoppingModallyCausesArgumentOutOfRangeException()
		{
			App.Tap("StartPopOnAppearingTest");
			App.WaitForNoElement("If this is visible, the PopOnAppearing test has passed.");
		}

		[Test]
		[Category(UITestCategories.Navigation)]
		[FailsOnIOS]
		public void Issue31366PushingWithModalStackCausesIncorrectStackOrder()
		{
			App.Tap("StartModalStackTest");
			App.WaitForNoElement("If this is visible, the modal stack test has passed.");
			App.Back();
		}
	}
}