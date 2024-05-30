#if ANDROID
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	class Issue22594 : _IssuesUITest
	{
		public Issue22594(TestDevice device) : base(device) { }

		public override string Issue => "[Android] Label.CharacterWrap wraps text too soon";

		[Test]
		public void CharacterWrapShouldNotWrapTextTooSoon()
		{
			_ = App.WaitForElement("label");

			// The test passes if character wrap doesn't wrap text too soon
			VerifyScreenshot();
		}
	}
}
#endif