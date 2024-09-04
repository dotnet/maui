#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue13634 : _IssuesUITest
	{
		public override string Issue => "Scrolling of Editor placed in ScollView does not work";
		public Issue13634(TestDevice testDevice) : base(testDevice)
		{
		}

		[Test]
		[Category(UITestCategories.ScrollView)]
		[Category(UITestCategories.Editor)]
		public void ScrollEditor()
		{
			App.WaitForElement("Editor");
			App.ScrollDown("Editor", ScrollStrategy.Programmatically);
			VerifyScreenshot();
		}
	}
}
#endif