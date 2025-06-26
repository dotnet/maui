#if TEST_FAILS_ON_WINDOWS
// https://github.com/dotnet/maui/issues/13027 In windows, .NET MAUI CollectionView does not reorder when grouped
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue28530 : _IssuesUITest
{
	public Issue28530(TestDevice device) : base(device) { }

	public override string Issue => "[Catalyst] CanMixGroups Set to False Still Allows Reordering Between Groups in CollectionView";

	[Fact]
	[Trait("Category", UITestCategories.CollectionView)]
	public void ReorderBetweenGroupsShouldNotOccurWhenCanMixGroupsIsFalse()
	{
		App.WaitForElement("CollectionViewControl");
		App.DragAndDrop("Item 2", "Item 3");
		VerifyScreenshot();
	}
}
#endif