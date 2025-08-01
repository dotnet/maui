using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28091SubscribeToUpdates : _IssuesUITest
{
	bool _isMetricsSupported;
	
	public override string Issue => "Add Layout Performance Profiler (SubscribeToUpdates)";

	public Issue28091SubscribeToUpdates(TestDevice device) : base(device)
	{
	}

	[SetUp]
	public void SetUp()
	{
		_isMetricsSupported = RuntimeFeature.IsMetricsSupported;
		RuntimeFeature.IsMetricsSupported = true;
	}

	[TearDown]
	public void TearDown()
	{
		RuntimeFeature.IsMetricsSupported = _isMetricsSupported;
	}

	/// <summary>
	/// This test verifies that layout performance updates are reflected in the HistoryLabel.
	/// It performs resizing actions and asserts that the label text is updated to include the profiling information.
	/// </summary>
	[Test]
	[Category(UITestCategories.Performance)]
	public void HistoryLabel_ShouldUpdate_AfterInteractions()
	{
		App.WaitForElement("WaitForStubControl");

		// Simulate layout changes
		App.Tap("IncreaseWidthButton");
		App.Tap("IncreaseHeightButton");
		
		// Capture updated history
		var updatedText = App.FindElement("HistoryLabel")?.GetText();

		// Validate history shows "passed" indicating performance updates were received
		Assert.That(updatedText, Is.Not.Null
			.And.EqualTo("Passed")
			.IgnoreCase);
	}
}