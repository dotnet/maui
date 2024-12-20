using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26629 : _IssuesUITest
	{
		public override string Issue => "ScrollView resizes when content changes";

		public Issue26629(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void ScrollViewResizesWhenContentChanges()
		{
			var sizeLabel = App.WaitForElement("SizeLabel");
			var initialSize = double.Parse(sizeLabel.GetText()!);
			var addLabelButton = App.WaitForElement("AddLabelButton");
			addLabelButton.Tap();
			App.WaitForElement("Label1");
			var newSize = double.Parse(sizeLabel.GetText()!);
			ClassicAssert.Greater(newSize, initialSize);
		}
	}
}
