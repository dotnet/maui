#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23488 : _IssuesUITest
	{
		public Issue23488(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Span text-decoration is incorrect whereas the Label behaves properly";

		[Test]
		[Category(UITestCategories.Label)]
		public void LabelHyperlinkUnderlineColor()
		{
			App.WaitForElement("WaitForLabelControl");
			VerifyScreenshot();
		}
	}
}
#endif