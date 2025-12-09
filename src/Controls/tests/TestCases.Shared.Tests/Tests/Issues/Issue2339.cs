using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2339 : _IssuesUITest
	{
		public Issue2339(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Picker not shown when .Focus() is called";

		[Test]
		[Category(UITestCategories.Picker)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnWindowsWhenRunningOnXamarinUITest("Focus Behavior is different")]
		public void FocusAndUnFocusMultipleTimes()
		{
			App.WaitForElement("btnFocusThenUnFocus");
			App.Tap("btnFocusThenUnFocus");
			App.WaitForElement("Picker Focused: 1");
			App.WaitForElement("Picker UnFocused: 1");
			App.Tap("btnFocusThenUnFocus");
			App.WaitForElement("Picker Focused: 2");
			App.WaitForElement("Picker UnFocused: 2");
		}
	}
}