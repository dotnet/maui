using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue186751 : _IssuesUITest
	{
		public Issue186751(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Editor IsReadOnly property prevent from modifying the text";

		[Test]
		public void EditorIsReadOnlyPreventModify()
		{
			App.WaitForElement("WaitForStubControl");

			// 1.The test fails if the placeholder text in the editor below is missing.
			App.Click("IsReadOnlyEditor");

			// 2. The test fails if the placeholder text in the editor below is not blue.
			VerifyScreenshot();
		}
	}
}