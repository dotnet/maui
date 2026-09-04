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
	[Category(UITestCategories.SafeAreaEdges)]
	public void RtlCollectionViewShouldNotBeTruncatedAfterLandscapeRotation()
	{
		try
		{
			App.WaitForElement("Issue36543CollectionView");
			App.SetOrientationLandscape();
			// Re-wait for the CollectionView after rotation, then let VerifyScreenshot's built-in
			// retry absorb orientation + safe-area inset settling (no fixed Thread.Sleep, which is
			// flaky on slow/notched emulators and always pays the full delay on fast runs).
			App.WaitForElement("Issue36543CollectionView");
			VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(5));
		}
		finally
		{
			App.SetOrientationPortrait();
		}
	}
}
#endif
