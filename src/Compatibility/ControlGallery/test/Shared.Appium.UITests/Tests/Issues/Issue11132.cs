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
		[Category(UITestCategories.CustomHandlers)]
		public void Issue11132CustomRendererLayerAndClip()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(InstructionsId);
			RunningApp.Screenshot("No crash");
		}
	}
}