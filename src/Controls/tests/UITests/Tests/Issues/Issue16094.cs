using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue16094 : _IssuesUITest
	{
		public Issue16094(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Shadows don't respect control shape";

		[Test]
		public void Issue16094Test()
		{
			App.WaitForElement("EditorControl");
			VerifyScreenshot();
		}
	}
}
