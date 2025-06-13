#if TEST_FAILS_ON_CATALYST // Need to fix the issue on Catalyst (https://github.com/dotnet/maui/issues/25826)
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8203 : _IssuesUITest
{
	public Issue8203(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "CollectionView fires SelectionChanged x (number of items selected +1) times, while incrementing SelectedItems from 0 " +
	"to number of items each time";

	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void SelectionChangedShouldBeRaisedOnceWhenSelectionChanges()
	{
		App.WaitForElement("one");
		App.Tap("one");
		App.Tap("two");
		Assert.Equal("SelectionChanged has been raised 2 times.", App.FindElement("SelectionChangedCount").GetText());
	}
}
#endif