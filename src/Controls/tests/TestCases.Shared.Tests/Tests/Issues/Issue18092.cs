using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    internal class Issue18092 : _IssuesUITest
    {
		public override string Issue => "Entry bug—height grows on every input event";
		public Issue18092(TestDevice testDevice) : base(testDevice)
		{
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void EntryHeight()
		{
			var entry = App.WaitForElement("Entry");
			App.EnterText("Entry", "E");
			var initialHeight = entry.GetRect().Height;
			App.EnterText("Entry", "ntry Control");
			var finalHeight = entry.GetRect().Height;
			Assert.That(initialHeight, Is.EqualTo(finalHeight));
		}
	}
}