#if MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues 
{
	public class Issue2728 : _IssuesUITest
	{
		const string LabelHome = "Hello Label";

		public Issue2728(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[macOS] Label FontAttributes Italic is not working";

		[Test]
		[Category(UITestCategories.Label)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnMac]
		public void Issue2728TestsItalicLabel()
		{
			App.WaitForNoElement(LabelHome);
			App.Screenshot("Label rendered with italic font");
		}
	}
}
#endif