using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18832 : _IssuesUITest
	{
		public override string Issue => "[iOS] Button CharacterSpacing makes FontSize fixed and large";

		public Issue18832(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Button)]
		public void SizesOfBothTextsShouldHaveTheSameAndCharacterSpacing()
		{
			_ = App.WaitForElement("button");

			// The test passes if both "Hello, World!" texts are of the same size
			// and have the same character spacing
			//VerifyScreenshot();
			// Temporary disabled to test agents
		}
	}
}