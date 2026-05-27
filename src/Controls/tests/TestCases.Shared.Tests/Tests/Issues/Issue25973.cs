using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue25973 : _IssuesUITest
	{
		public override string Issue => "Editor vertical text alignment not working after toggling IsVisible";

		public Issue25973(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Editor)]
		public void VerifyEditorVerticalTextAlignmentWhenVisibilityToggled()
		{
			App.WaitForElement("ToggleButton");
			App.Click("ToggleButton");
			VerifyScreenshot();
		}
	}
}
