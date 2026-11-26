#if ANDROID || IOS // SafeAreaEdges landscape notch regression is Android/iOS-only (dotnet/maui#35410)
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35410 : _IssuesUITest
{
	public Issue35410(TestDevice device) : base(device)
	{
	}

	public override string Issue => "ScrollView content is obscured by the notch after rotating to landscape";

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void TextRemainsVisibleAfterLandscapeRotation()
	{
		App.WaitForElement("Issue35410SentenceEditor");

		try
		{
			App.SetOrientationLandscape();
			var notchSensitiveLabel = App.WaitForElement("Issue35410NotchSensitiveLabel");
			var labelX = notchSensitiveLabel.GetRect().X;
			TestContext.Progress.WriteLine($"D6 notch-sensitive label X after rotation: {labelX}");
			if (Device == TestDevice.iOS)
			{
				Assert.That(
					labelX,
					Is.GreaterThan(0),
					"The D6 label should be inset from the landscape notch on iOS.");
			}

			VerifyScreenshot("Issue35410_Landscape", retryTimeout: TimeSpan.FromSeconds(2));
		}
		finally
		{
			App.SetOrientationPortrait();
		}
	}
}
#endif
