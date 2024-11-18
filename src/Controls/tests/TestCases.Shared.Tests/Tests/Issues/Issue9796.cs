using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue9796 : _IssuesUITest
	{
		public override string Issue => "[Android]Editor controls don't raise Completed event consistently";

		public Issue9796(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Editor)]
		[Category(UITestCategories.Focus)]
		public void EditorCompletedEventShouldTriggerOnFocusLost()
		{
			// Is a Android issue; see https://github.com/dotnet/maui/issues/9796
			App.WaitForElement("FocusButton");
			App.Click("FocusButton");
			App.Click("UnfocusButton");
			ClassicAssert.AreEqual("Triggered", App.FindElement("Label").GetText());
		}
	}
}
