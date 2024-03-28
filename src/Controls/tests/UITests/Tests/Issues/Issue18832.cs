using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18832 : _IssuesUITest
	{
		public override string Issue => "[iOS] Button CharacterSpacing makes FontSize fixed and large";

		public Issue18832(TestDevice device) : base(device)
		{
		}

		[Test]
		public void SizesOfBothTextsShouldHaveTheSameAndCharacterSpacing()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows });

			_ = App.WaitForElement("button");

			// The test passes if both "Hello, World!" texts are of the same size
			// and have the same character spacing
			VerifyScreenshot();
		}
	}
}
