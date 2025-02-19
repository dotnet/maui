using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18675 : _IssuesUITest
	{
		public Issue18675(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Editor IsReadOnly property prevent from modifying the text";

		[Test]
		[Category(UITestCategories.Editor)]
		[FailsOnMacWhenRunningOnXamarinUITest("VerifyScreenshot method not implemented on macOS")]
		public async Task EditorIsReadOnlyPreventModify()
		{
			App.WaitForElement("WaitForStubControl");

			// 1.The test fails if the placeholder text in the editor below is missing.
			App.Tap("IsReadOnlyEditor");

			// Delay for the Editor underline on Android to return from
			// the selected state to normal state.
			await Task.Delay(500);

			// 2. The test fails if the placeholder text in the editor below is not blue.
			VerifyScreenshot();
		}
	}
}