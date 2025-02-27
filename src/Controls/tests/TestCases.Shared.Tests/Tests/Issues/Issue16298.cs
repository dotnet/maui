using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue16298 : _IssuesUITest
	{
		public override string Issue => "keyboard should dismiss on unfocused event";

		public Issue16298(TestDevice testDevice) : base(testDevice)
		{
		}

		[Test]
		[Category(UITestCategories.Shell)]
		public void Issue16298KeyboardDismissonSearchHander()
		{
			App.WaitForElement("FocusBtn");
			App.Click("FocusBtn");
			bool IskeyboardVisible = App.IsKeyboardShown();
			Assert.That(IskeyboardVisible, Is.True);

			App.WaitForElement("UnFocusBtn");
			App.Click("UnFocusBtn");

			IskeyboardVisible = App.IsKeyboardShown();
			Assert.That(IskeyboardVisible, Is.False);
		}
	}
}