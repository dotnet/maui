using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18754 : _IssuesUITest
	{
		public Issue18754(TestDevice device) : base(device) { }

		public override string Issue => "[D9] Editor IsReadOnly works";

		[Test]
		[Category(UITestCategories.Editor)]
		[FailsOnMac("Currently IsKeyboardShown is not implemented.")]
		[FailsOnWindows("Currently IsKeyboardShown is not implemented.")]
		public void Issue18754Test()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. Attempt to enter any text into the editor below.
			App.Tap("ReadOnlyEditor");

			// 2. The test fails if the editor displays the input.
			ClassicAssert.IsFalse(App.IsKeyboardShown());

			// 3. Attempt to edit the text in the editor below.
			App.Tap("FilledReadOnlyEditor");

			// 4. The test fails if the editor displays the input.
			ClassicAssert.IsFalse(App.IsKeyboardShown());
		}
	}
}