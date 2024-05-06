using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18754 : _IssuesUITest
	{
		public Issue18754(TestDevice device) : base(device) { }

		public override string Issue => "[D9] Editor IsReadOnly works";

		[Test]
		[Category(UITestCategories.Editor)]
		public void Issue18754Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Mac, TestDevice.Windows },
				"Currently IsKeyboardShown is not implemented.");

			App.WaitForElement("WaitForStubControl");

			// 1. Attempt to enter any text into the editor below.
			App.Click("ReadOnlyEditor");

			// 2. The test fails if the editor displays the input.
			Assert.IsFalse(App.IsKeyboardShown());

			// 3. Attempt to edit the text in the editor below.
			App.Click("FilledReadOnlyEditor");

			// 4. The test fails if the editor displays the input.
			Assert.IsFalse(App.IsKeyboardShown());
		}
	}
}
