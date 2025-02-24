#if TEST_FAILS_ON_WINDOWS //For more information, see : https://github.com/dotnet/maui/issues/27899
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue935 : _IssuesUITest
	{
		public Issue935(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ViewCell.ItemTapped only fires once for ListView.SelectedItem";

		[Test]
		[Description("Verify that OnTapped is fired every time a ViewCell is tapped")]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Compatibility)]
		public void Issue935TestsMultipleOnTappedViewCell()
		{
			App.WaitForElement("TestLabel");
			App.Tap("TestLabel");
			var label = App.WaitForElement("TestLabel").GetText();
			Assert.That("I have been selected:1", Is.EqualTo(label));
			App.Tap("TestLabel");
			var label1 = App.WaitForElement("TestLabel").GetText();
			Assert.That("I have been selected:2", Is.EqualTo(label1));
		}
	}
}
#endif