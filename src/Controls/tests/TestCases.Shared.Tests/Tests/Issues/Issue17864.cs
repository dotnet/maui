using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue17864 : _IssuesUITest
	{
		const string CollectionView = "CollectionView";
		public Issue17864(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[Windows] CollectionView throws NRE when value of IsGrouped property is changed to false";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewShouldNotCrashWhenIsGroupedChangesInBothDirections()
		{
			App.WaitForElement(CollectionView);

			// true -> false
			App.Tap("Button");
			App.WaitForElement(CollectionView);

			// false -> true while items remain flat
			App.Tap("EnableGroupingOnlyButton");
			App.WaitForElement(CollectionView);
		}
	}
}