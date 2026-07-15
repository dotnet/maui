#if ANDROID // SafeAreaEdges landscape-cutout RTL displacement is Android-only (dotnet/maui#36543)
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36543 : _IssuesUITest
{
	public Issue36543(TestDevice device) : base(device) { }

	public override string Issue => "[Android] RTL CollectionView content is truncated / shifted into the display cutout after landscape rotation";

	[Test]
	// TODO: Remove [Material3] once dotnet/maui#31631 merges. That PR adds a dedicated SafeAreaEdges stage on Pixel 3 XL API 36; until then, [Material3] is the only category that routes this test to a notched device (required to reproduce this bug).
	[Category(UITestCategories.Material3)]
	public void RtlCollectionViewShouldNotBeTruncatedAfterLandscapeRotation()
	{
		try
		{
			App.WaitForElement("Issue36543CollectionView");
			App.SetOrientationLandscape();
			// Allow orientation change + safe-area inset dispatch to settle before capturing.
			Thread.Sleep(2000);
			App.WaitForElement("Issue36543CollectionView");
			VerifyScreenshot();
		}
		finally
		{
			App.SetOrientationPortrait();
		}
	}
}
#endif
