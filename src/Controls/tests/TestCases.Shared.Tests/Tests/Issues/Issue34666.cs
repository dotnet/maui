#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34666 : _IssuesUITest
{
	public Issue34666(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Disabling RefreshView cascades IsEnabled=false to its child CollectionView, preventing scrolling";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewDoesNotScrollWhenRefreshViewDisabled()
	{
		App.WaitForElement("Baboon");
		App.ScrollDown("CollectionView");
		App.ScrollDown("CollectionView");
		App.WaitForElement("Baboon");
	}
}
#endif