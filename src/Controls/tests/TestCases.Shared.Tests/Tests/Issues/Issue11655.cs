using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue11655 : _IssuesUITest
	{
		public Issue11655(TestDevice device) : base(device) { }

		public override string Issue => "Label's HorizontalTextAlignment property is not updated properly at runtime";

		[Test]
		[Category(UITestCategories.Label)]
		public void EntrySelectionLengthRuntimeUpdate()
		{
			App.WaitForElement("Button");
			App.Tap("Button");
			VerifyScreenshot();
		}

	}
}
