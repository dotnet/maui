using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23630 : _IssuesUITest
	{
		public Issue23630(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Shadow not visible in Button When using Clipping";

		[Test]
		[Category(UITestCategories.Visual)]
		public void Issue23630Test()
		{
			App.WaitForElement("visualElement");
			VerifyScreenshot();
		}
	}
}
