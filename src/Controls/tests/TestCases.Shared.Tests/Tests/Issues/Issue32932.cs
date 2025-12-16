using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32932 : _IssuesUITest
{
	public Issue32932(TestDevice device) : base(device)
	{
	}

	public override string Issue => "[Android] EmptyView doesn’t display when CollectionView is placed inside a VerticalStackLayout";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void EmptyViewShouldDisplayWhenCollectionViewIsInsideVerticalStackLayout()
	{
		App.WaitForElement("EmptyView");
		App.WaitForElement("EmptyViewLabel");
	}
}