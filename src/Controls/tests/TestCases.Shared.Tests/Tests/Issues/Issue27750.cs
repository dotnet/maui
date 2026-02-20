using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27750 : _IssuesUITest
	{
		public override string Issue => "[iOS] Editor scrolled to the bottom when tapped while inside the ScrollView";

		public Issue27750(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.Editor)]
		public void EditorShouldNotMoveToBottom()
		{
			App.WaitForElement("Editor");
			App.Tap("Editor");
#if IOS
			VerifyScreenshot(cropBottom:1400);
#else
			VerifyScreenshot();
#endif
		}
	}
}
