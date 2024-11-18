#if !MACCATALYST
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23333 : _IssuesUITest
	{
		public override string Issue => "Frame offsets inner content view by 1pt";

		public Issue23333(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Frame)]
		public void ValidateFrameOffsets()
		{
			App.WaitForElement("FrameWithImage");
			VerifyScreenshot();
		}
	}
}
#endif