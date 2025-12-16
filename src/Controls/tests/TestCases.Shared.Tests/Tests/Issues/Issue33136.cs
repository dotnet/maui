#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS   // TitleBar is only applicable for Windows and macOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
	public class Issue33136 : _IssuesUITest
	{
		public Issue33136(TestDevice device) : base(device) { }

		public override string Issue => "TitleBar Content Overlapping with Traffic Light Buttons on Latest macOS Version";
		[Test]
		[Category(UITestCategories.Window)]
		public void VerifyTitleBarAlignment()
		{
			App.WaitForElement("TitleBarAlignmentLabel");
			VerifyScreenshot(includeTitleBar: true);
		}
	}
#endif