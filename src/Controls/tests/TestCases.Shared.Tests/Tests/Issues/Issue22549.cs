using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22549 : _IssuesUITest
	{
		public Issue22549(TestDevice device) : base(device) { }

		public override string Issue => "Binding Border.StrokeShape not working";

		[Test]
		[Category(UITestCategories.Border)]
		public void BindingOnStrokeShapeShouldWork()
		{
			Exception? exception = null;

			App.WaitForElement("button");

			// Border should have radius
			VerifyScreenshotOrSetException(ref exception, "BindingOnStrokeShapeWithRadius");

			App.Click("button");

			// The test passes if border radius is equal to 0
			VerifyScreenshotOrSetException(ref exception, "BindingOnStrokeShapeWithoutRadius");

			if (exception is not null)
			{
				throw exception;
			}
		}
	}
}