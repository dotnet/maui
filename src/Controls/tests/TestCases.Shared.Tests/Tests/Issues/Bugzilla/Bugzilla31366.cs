#if TEST_FAILS_ON_WINDOWS // Fails on windows, More information :https://github.com/dotnet/maui/issues/24243
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
		public void Issue31366PushingAndPoppingModallyCausesArgumentOutOfRangeException()
		{
			App.WaitForElement("StartPopOnAppearingTest");
			App.Tap("StartPopOnAppearingTest");
			App.WaitForElement("If this is visible, the PopOnAppearing test has passed.");
		}

		[Test]
		[Category(UITestCategories.Navigation)]
		public void Issue31366PushingWithModalStackCausesIncorrectStackOrder()
		{
			App.WaitForElement("StartModalStackTest");
			App.Tap("StartModalStackTest");
			App.WaitForElement("If this is visible, the modal stack test has passed.");
			App.Back();
		}
	}
}
#endif