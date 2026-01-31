using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33688 : _IssuesUITest
{
	public override string Issue => "BackButtonBehavior is no longer triggered once a ContentPage contains a CollectionView and the ItemsSource has been changed";

	public Issue33688(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Shell)]
	public void BackButtonBehaviorTriggersWithCollectionView()
	{
		// Wait for main page to load
		App.WaitForElement("NavigateButton");

		// Navigate to the second page with CollectionView
		App.Tap("NavigateButton");

		// Wait for the second page to load - use StatusLabel as primary indicator
		App.WaitForElement("StatusLabel");
		
		// Find and tap the filter button to load items - this triggers the bug
		// (setting ItemsSource to a new ObservableCollection)
		App.WaitForElement("FilterButton");
		App.Tap("FilterButton");

		// Give time for the CollectionView to update
		App.WaitForElement("TestCollectionView");

		// Press the back button
		App.TapBackArrow();

		// Wait for navigation back and verify BackButtonBehavior was triggered
		App.WaitForElement("ResultLabel");
		
		var resultText = App.FindElement("ResultLabel").GetText();
		Assert.That(resultText, Is.EqualTo("BackButtonBehavior triggered!"), 
			"BackButtonBehavior command should have been executed when pressing back button");
	}
}
