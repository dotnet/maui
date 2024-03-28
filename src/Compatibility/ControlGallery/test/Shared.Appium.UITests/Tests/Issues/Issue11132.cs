using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue11132 : IssuesUITest
	{
		const string InstructionsId = "instructions";

		public Issue11132(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] [iOS] UpdateClip throws NullReferenceException when the Name of the Mask of the Layer is null";

		[Test]
		[Description("Verify that can use a CustomRenderer overriding the iOS View Layer properties")]
		[Category(UITestCategories.Frame)]
		[Category(UITestCategories.CustomHandlers)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void Issue11132CustomRendererLayerAndClip()
		{
			RunningApp.WaitForElement(InstructionsId);
			RunningApp.Screenshot("No crash");
		}
	}
}