#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    internal class Issue9796 : _IssuesUITest
	{
		public override string Issue => "[Android]Editor/Entry controls don't raise Completed event consistently";

		public Issue9796(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Editor)]
		[Category(UITestCategories.Entry)]
		[Category(UITestCategories.Focus)]
		public void EntryAndEditorCompletedEventShouldTriggerOnFocusLost()
		{
			// Is a Android issue; see https://github.com/dotnet/maui/issues/9796
			App.WaitForElement("EditorControl");
            App.WaitForElement("EntryControl");
            App.Tap("EntryControl");
            App.Tap("EditorControl");
            App.Tap("EntryControl");
			VerifyScreenshot();
		}
	}
}
#endif
