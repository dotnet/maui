using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue2963 : IssuesUITest
	{
		readonly string EditorId = "DisabledEditor";
		readonly string FocusedLabelId = "FocusedLabel";

		public Issue2963(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Disabling Editor in iOS does not disable entry of text";

		[Test]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void Issue2963Test()
		{
			RunningApp.Screenshot("I am at Issue 2963");
			RunningApp.Tap(EditorId);
			ClassicAssert.AreEqual("False", RunningApp.FindElement(FocusedLabelId).GetText());
			RunningApp.Screenshot("Label should still be false");
		}
	}
}