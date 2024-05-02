using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue16094 : _IssuesUITest
	{
		public Issue16094(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Shadows don't respect control shape";

		[Test]
		[Category(UITestCategories.Editor)]
		public void ShadowsDontRespectControlShape()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Mac });

			App.WaitForElement("TestScrollView");
			App.ScrollDown("TestScrollView", ScrollStrategy.Gesture);
			App.WaitForElement("EditorControl");
			VerifyScreenshot();
		}
	}
}
