using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18754 : _IssuesUITest
	{
		public Issue18754(TestDevice device) : base(device) { }

		public override string Issue => "[D9] Editor IsReadOnly works";

		[Fact]
		[Trait("Category", UITestCategories.Editor)]
		[FailsOnMacWhenRunningOnXamarinUITest("Currently IsKeyboardShown is not implemented.")]
		[FailsOnWindowsWhenRunningOnXamarinUITest("Currently IsKeyboardShown is not implemented.")]
		public void Issue18754Test()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. Attempt to enter any text into the editor below.
			App.Tap("ReadOnlyEditor");

			// 2. The test fails if the editor displays the input.
			Assert.False(App.IsKeyboardShown());

			// 3. Attempt to edit the text in the editor below.
			App.Tap("FilledReadOnlyEditor");

			// 4. The test fails if the editor displays the input.
			Assert.False(App.IsKeyboardShown());
		}
	}
}