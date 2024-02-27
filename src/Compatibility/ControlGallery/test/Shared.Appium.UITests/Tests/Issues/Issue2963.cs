using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
    internal class Issue2963 : IssuesUITest
	{
		readonly string EditorId = "DisabledEditor";
		readonly string FocusedLabelId = "FocusedLabel";

		public Issue2963(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Disabling Editor in iOS does not disable entry of text";

		[Test]
		public void Issue2963Test()
		{
			App.Screenshot("I am at Issue 2963");
			App.Click(EditorId);
			ClassicAssert.AreEqual("False", App.FindElement(FocusedLabelId).GetText());
			App.Screenshot("Label should still be false");
		}
	}
}