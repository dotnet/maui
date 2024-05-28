#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22286 : _IssuesUITest
	{
		public Issue22286(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "The iOS keyboard is not fully retracted and requires an extra click on the Done button";

		[Test]
		public void ClosedKeyboardNoToolbarVisible()
		{
			App.WaitForElement("TestEditor1");
			App.Tap("TestEditor1");

			App.Tap("TestEditor2");

			if (App.IsKeyboardShown())
				App.DismissKeyboard();

			// Closing the keyboard also hide the custom Done Button in the InputAccessoryView.
			App.Screenshot("ClosedKeyboardNoToolbarVisible");
		}
	}
}
#endif