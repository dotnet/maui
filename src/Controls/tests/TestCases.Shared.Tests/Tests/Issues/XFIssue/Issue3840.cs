using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3840 : _IssuesUITest
{
	public Issue3840(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] Translation change causes ScrollView to reset to initial position (0, 0)";

	// TODO: The _ variables need to be AutomationId values
//		[Test]
//		[FailsOnIOS]
//		[Category(UITestCategories.ScrollView)]
//		public void TranslatingViewKeepsScrollViewPosition()
//		{
//			RunningApp.WaitForElement(_failedText);
//			RunningApp.Tap(_button1);
//			RunningApp.Tap(_button2);
//#if WINDOWS
//			var label = RunningApp.WaitForElement(_failedText);
//			Assert.AreEqual(0, label[0].Rect.Height);
//			Assert.AreEqual(0, label[0].Rect.Width);
//#else
//		var result = RunningApp.QueryUntilNotPresent(() => RunningApp.Query(_failedText));
//#endif
//		}
}