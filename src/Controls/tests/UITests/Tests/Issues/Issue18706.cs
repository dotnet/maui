using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18706 : _IssuesUITest
	{
		public Issue18706(TestDevice device) : base(device) { }

		public override string Issue => "Editor Background works";

		[Test]
		public void EditorBackgroundWorks()
		{
			App.WaitForElement("WaitForStubControl");

			VerifyScreenshot();
		}
	}
}