#if TEST_FAILS_ON_WINDOWS // The issue also affects Windows; tracked for follow-up in: https://github.com/dotnet/maui/issues/34701
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34666 : _IssuesUITest
{
	public Issue34666(TestDevice device) : base(device)
	{
	}

	public override string Issue => "The C6 page cannot scroll on Windows and Android platforms";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewScrollsWhenRefreshViewDisabled()
	{
		App.WaitForElement("Baboon");
		App.ScrollDown("CollectionView");
		App.ScrollDown("CollectionView");
		App.WaitForElement("Gelada");
	}
}
#endif