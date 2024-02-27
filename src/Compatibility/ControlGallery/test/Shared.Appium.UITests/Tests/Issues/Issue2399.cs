using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;


namespace UITests
{
    internal class Issue2399 : IssuesUITest
	{
		const string AllEventsHaveDetached = "AllEventsHaveDetached";

		public Issue2399(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Label Renderer Dispose never called";

		[Test]
		public void ChildAddedShouldFire()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement(AllEventsHaveDetached);
			var text = App.FindElement(AllEventsHaveDetached).GetText();
			ClassicAssert.NotNull(text);
			ClassicAssert.AreEqual("Success", text);
		}
	}
}