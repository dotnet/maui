using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21913 : _IssuesUITest
{
	public Issue21913(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Grouped CollectionView items leave stale space when MaximumHeightRequest changes";

#if IOS
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void FirstGroupSecondToggleShouldMoveSecondHeaderBackDownOnIOS()
	{
	   App.WaitForElement("FirstGroupButton");
	   App.WaitForElement("SecondGroupButton");
		
		App.Tap("FirstGroupButton");
		Task.Delay(500).Wait(); // Allow time for the UI to update after collapsing the first group

		App.Tap("FirstGroupButton");
		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

#if ANDROID
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void SecondGroupSecondToggleShouldMoveThirdHeaderBackDownOnAndroid()
	{
		App.WaitForElement("ThirdGroupButton");
		App.Tap("ThirdGroupButton");
		Task.Delay(500).Wait(); // Allow time for the UI to update after collapsing the second group
        
		App.Tap("ThirdGroupButton");
		Task.Delay(500).Wait(); // Allow time for the UI to update after expanding the second group
		VerifyScreenshot();

	}
#endif
}
