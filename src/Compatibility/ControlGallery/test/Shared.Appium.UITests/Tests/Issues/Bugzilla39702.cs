
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla39702 : IssuesUITest
	{
		const string TheEntry = "TheEntry";
		const string Success = "Success";

		public Bugzilla39702(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Cannot enter text when Entry is focus()'d from an editor completed event";

		[Test]
		[Category(UITestCategories.Entry)]
		[Category(UITestCategories.Focus)]
		[FailsOnIOS]
		public async Task ControlCanBeFocusedByUnfocusedEvent()
		{
			RunningApp.WaitForElement(TheEntry);
			await Task.Delay(4000);
			RunningApp.EnterText(TheEntry, Success); // Should be typing into the Entry at this point
			RunningApp.WaitForNoElement(Success);
		}
	}
}