using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25581 : _IssuesUITest
	{
		public override string Issue => "Editor Scaling creating new lines on older iOS versions";

		public Issue25581(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.Editor)]
		public void VerifyDynamicEditorScaling()
		{
			App.WaitForElement("Button");
			App.Tap("Button");

			VerifyScreenshot();
		}
	}
}
