#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19152 : _IssuesUITest
	{
		public Issue19152(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Windows | Entry ClearButton not taking color of text";

		[Test]
		[Category(UITestCategories.Entry)]
		public void EntryClearButtonColorShouldMatchTextColor()
		{
			App.WaitForElement("entry");
			App.Tap("button");
			VerifyScreenshot();
		}
	}
}
#endif

