#if !WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21983 : _IssuesUITest
	{
		public override string Issue => "GradientBrushes are not supported on Shape.Stroke";

		public Issue21983(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Shape)]
		public void GradientShouldBeAppliedToStrokes()
		{
			_ = App.WaitForElement("path");

			VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
		}
	}
}
#endif
