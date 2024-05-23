#if IOS
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2399 : _IssuesUITest
	{
		const string AllEventsHaveDetached = "AllEventsHaveDetached";

		public Issue2399(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Label Renderer Dispose never called";

		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.Label)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOS]
		public void WaitForAllEffectsToDetach()
		{
			App.WaitForElement(AllEventsHaveDetached);
			var text = App.FindElement(AllEventsHaveDetached).GetText();
			ClassicAssert.NotNull(text);
			ClassicAssert.AreEqual("Success", text);
		}
	}
}
#endif