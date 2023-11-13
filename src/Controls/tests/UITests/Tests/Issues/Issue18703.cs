using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18703 : _IssuesUITest
	{
		public Issue18703(TestDevice device) : base(device) { }

		public override string Issue => "Editor TextAlignment properties works";

		[Test]
		public void EditorTextAlignmentWorks()
		{
			App.WaitForElement("WaitForStubControl");

			VerifyScreenshot();
		}
	}
}