#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS // This test is only applicable for iOS and Catalyst platforms.
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue11132 : _IssuesUITest
	{
		const string InstructionsId = "instructions";

		public Issue11132(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] [iOS] UpdateClip throws NullReferenceException when the Name of the Mask of the Layer is null";

		[Fact]
		[Trait("Category", UITestCategories.Compatibility)]
		public void Issue11132CustomRendererLayerAndClip()
		{
			App.WaitForElement(InstructionsId);
		}
	}
}
#endif