#if TEST_FAILS_ON_WINDOWS //This span.LineHeight property has no effect on windows, For more Information - https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/label?view=net-maui-9.0#use-formatted-text
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19592 : _IssuesUITest
	{
		public override string Issue => "Span LineHeight Wrong on Android";

		public Issue19592(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Label)]
		public void SpanLineHeightShouldNotGrowProgressively()
		{
			_ = App.WaitForElement("label");

			// The line height should be the same for each line
			// of the paragraph, 1.5 and 2.5 respectively,
			// as opposed to progressively growing
			VerifyScreenshot();
		}
	}
}
#endif