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
		public void EditorIsReadOnlyPreventModify()
		{
			App.WaitForElement("WaitForStubControl");

			// 1.The test fails if the placeholder text in the editor below is missing.
			App.Tap("IsReadOnlyEditor");

			// Delay for the Editor underline on Android to return from
			// the selected state to normal state.


			// 2. The test fails if the placeholder text in the editor below is not blue.
			VerifyScreenshot();
		}
	}
}