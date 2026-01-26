using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30363 : _IssuesUITest
{
	public override string Issue => "[iOS] CollectionView does not clear selection when SelectedItem is set to null";

	public Issue30363(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewSelectionShouldClear()
	{
		App.WaitForElement("cvItem");
		App.Tap("cvItem");
		VerifyScreenshot();
	}
}