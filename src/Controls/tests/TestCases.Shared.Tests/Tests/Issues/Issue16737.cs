using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue16737 : _IssuesUITest
	{
		public Issue16737(TestDevice device) : base(device) { }

		public override string Issue => "Title colour on Android Picker, initially appears grey";

		[Test]
		[Category(UITestCategories.Picker)]
		public void Issue16737Test()
		{
			_ = App.WaitForElement("label");

			// The test passes if the title's color is red
			VerifyScreenshot();
		}
	}
}