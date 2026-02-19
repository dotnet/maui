#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26158 : _IssuesUITest
	{
		public Issue26158(TestDevice device) : base(device) { }

		public override string Issue => "SelectionLength property not applied when an entry is focused";

		[Test]
		[Category(UITestCategories.Entry)]
		public void SelectionLengthShouldUpdateWhenEntryIsFocused()
		{
			App.WaitForElement("entry");
			App.Click("entry");
#if ANDROID
			App.DismissKeyboard();
#endif
			VerifyScreenshot();
		}
	}
}
#endif