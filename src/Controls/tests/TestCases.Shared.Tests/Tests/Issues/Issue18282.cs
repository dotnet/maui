using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	class Issue18282 : _IssuesUITest
	{
		public Issue18282(TestDevice device) : base(device) { }

		public override string Issue => "The editor placeholder can't able to changed, It's default placed at center";

		[Test]
		[Category(UITestCategories.Editor)]
		public void EditorPlaceholderPosition()
		{
			App.WaitForElement("WaitForStubControl");

			VerifyScreenshot();
		}
	}
}
