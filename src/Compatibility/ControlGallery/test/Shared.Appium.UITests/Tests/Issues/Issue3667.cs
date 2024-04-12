using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue3667 : IssuesUITest
	{
		string text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

		public Issue3667(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Enhancement] Add text-transforms to Label";

		[Test]
		[Category(UITestCategories.Label)]
		[FailsOnIOS]
		public void Issue3667Tests()
		{
			RunningApp.WaitForNoElement(text);
			RunningApp.Tap("ChangeTextTransformButton");
			RunningApp.WaitForNoElement(text);
			RunningApp.Tap("ChangeTextTransformButton");
			RunningApp.WaitForNoElement(text.ToLowerInvariant());
			RunningApp.Tap("ChangeTextTransformButton");
			RunningApp.WaitForNoElement(text.ToUpperInvariant());
			RunningApp.Tap("ChangeTextTransformButton");
			RunningApp.WaitForNoElement(text);
		}
	}
}