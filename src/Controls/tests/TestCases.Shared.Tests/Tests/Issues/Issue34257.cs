#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS //In windows, related issue: https://github.com/dotnet/maui/issues/4715
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34257 : _IssuesUITest
{
	public Issue34257(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "CollectionView vertical grid item spacing updates all rows and columns";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void UpdatingHorizontalSpacingShouldResizeBothColumns()
	{
		var firstColumnBefore = App.WaitForElement("FirstColumnTopItem").GetRect();
		App.Tap("ApplyHorizontalSpacingButton");
		App.WaitForElement("StatusLabel", "Spacing=0,80");
		var firstColumnAfter = App.WaitForElement("FirstColumnTopItem").GetRect();
		Assert.That(firstColumnBefore.X, Is.Not.EqualTo(firstColumnAfter.X), $"Expected the first column to move");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void UpdatingVerticalSpacingShouldResizeBothRows()
	{
		var firstColumnBefore = App.WaitForElement("FirstColumnBottomItem").GetRect();
		App.Tap("ApplyVerticalSpacingButton");
		App.WaitForElement("StatusLabel", "Spacing=40,0");
		var firstColumnAfter = App.WaitForElement("FirstColumnBottomItem").GetRect();
		Assert.That(firstColumnBefore.Y, Is.Not.EqualTo(firstColumnAfter.Y), $"Expected the second row to move");
	}
}
#endif