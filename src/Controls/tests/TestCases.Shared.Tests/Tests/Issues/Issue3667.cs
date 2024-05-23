using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3667 : _IssuesUITest
	{
		string text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

		public Issue3667(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Enhancement] Add text-transforms to Label";

		[Test]
		[Category(UITestCategories.Label)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOS]
		public void Issue3667Tests()
		{
			App.WaitForNoElement(text);
			App.Tap("ChangeTextTransformButton");
			App.WaitForNoElement(text);
			App.Tap("ChangeTextTransformButton");
			App.WaitForNoElement(text.ToLowerInvariant());
			App.Tap("ChangeTextTransformButton");
			App.WaitForNoElement(text.ToUpperInvariant());
			App.Tap("ChangeTextTransformButton");
			App.WaitForNoElement(text);
		}
	}
}