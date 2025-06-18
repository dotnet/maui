using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2963 : _IssuesUITest
	{
		readonly string EditorId = "DisabledEditor";
		readonly string FocusedLabelId = "FocusedLabel";

		public Issue2963(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Disabling Editor in iOS does not disable entry of text";

		[Test]
		[Category(UITestCategories.Compatibility)]
		public void Issue2963Test()
		{
			App.WaitForElement(EditorId);
			App.Tap(EditorId);
			Assert.That(App.FindElement(FocusedLabelId).GetText(), Is.EqualTo("False"));
		}
	}
}