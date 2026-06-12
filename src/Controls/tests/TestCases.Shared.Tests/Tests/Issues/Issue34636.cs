using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34636 : _IssuesUITest
{
	public Issue34636(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "CollectionView ItemSpacing - First and last item on the list is truncated after changing Spacing value";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerticalItemsRemainFullyVisibleAfterChangingSpacing()
	{
		App.WaitForElement("ChangeSpacingButton");
		App.Tap("ChangeSpacingButton");
		VerifyScreenshot();
	}
}
