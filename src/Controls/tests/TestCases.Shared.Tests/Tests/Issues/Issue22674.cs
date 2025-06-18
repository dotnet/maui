using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22674 : _IssuesUITest
{
	public Issue22674(TestDevice device) : base(device) { }

	public override string Issue => "Crash when quickly clicking to delete item";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void RemoveItemWhenSelectionChanged()
	{
		App.WaitForElement("TestCollectionView");

		for (int i = 0; i < 5; i++)
		{
			App.TapCoordinates(24, 24);
		}

		// Without crashes, the test has passed.
	}
}
