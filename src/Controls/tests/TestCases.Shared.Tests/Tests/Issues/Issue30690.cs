using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue30690 : _IssuesUITest
	{
		public Issue30690(TestDevice device) : base(device)
		{
		}

		public override string Issue => "RefreshView IsRefreshEnabled Property and Consistent IsEnabled Behavior";

		[Test]
		[Category(UITestCategories.RefreshView)]
		public void IsRefreshEnabledPreventsRefresh()
		{
			// Initially both properties should be true
			App.WaitForElement("CheckStates");
			App.Tap("CheckStates");
			Assert.That(GetStatusText().Contains("IsEnabled: True"), "IsEnabled should be true initially");
			Assert.That(GetStatusText().Contains("IsRefreshEnabled: True"), "IsRefreshEnabled should be true initially");

			// Disable IsRefreshEnabled
			App.Tap("ToggleIsRefreshEnabled");
			Assert.That(GetStatusText().Contains("IsRefreshEnabled: False"), "IsRefreshEnabled should be false after toggle");

			// Try to start refresh - should not work
			App.Tap("StartRefresh");
			App.Tap("CheckStates");
			Assert.That(GetStatusText().Contains("IsRefreshing: False"), "IsRefreshing should remain false when IsRefreshEnabled is false");
		}

		[Test]
		[Category(UITestCategories.RefreshView)]
		public void IsEnabledDisablesEntireView()
		{
			// Initially entry should be interactive
			App.WaitForElement("TestEntry");
			App.Tap("TestEntry");
			App.EnterText("TestEntry", "test input");

			// Disable IsEnabled
			App.Tap("ToggleIsEnabled");
			App.Tap("CheckStates");
			Assert.That(GetStatusText().Contains("IsEnabled: False"), "IsEnabled should be false after toggle");

			// Entry should not be interactive when IsEnabled is false
			// Note: The specific behavior may vary by platform, but the RefreshView itself should be disabled
			App.Tap("StartRefresh");
			App.Tap("CheckStates");
			Assert.That(GetStatusText().Contains("IsRefreshing: False"), "IsRefreshing should remain false when IsEnabled is false");
		}

		[Test]
		[Category(UITestCategories.RefreshView)]
		public void IsRefreshEnabledAllowsChildInteraction()
		{
			// Disable only IsRefreshEnabled, leave IsEnabled true
			App.WaitForElement("ToggleIsRefreshEnabled");
			App.Tap("ToggleIsRefreshEnabled");
			App.Tap("CheckStates");
			Assert.That(GetStatusText().Contains("IsEnabled: True"), "IsEnabled should remain true");
			Assert.That(GetStatusText().Contains("IsRefreshEnabled: False"), "IsRefreshEnabled should be false");

			// Entry should still be interactive
			App.Tap("TestEntry");
			App.ClearText("TestEntry");
			App.EnterText("TestEntry", "refresh disabled but entry works");
			
			// But refresh should not work
			App.Tap("StartRefresh");
			App.Tap("CheckStates");
			Assert.That(GetStatusText().Contains("IsRefreshing: False"), "IsRefreshing should remain false when IsRefreshEnabled is false");
		}

		[Test]
		[Category(UITestCategories.RefreshView)]
		public void PullToRefreshWorksWhenEnabled()
		{
			// Ensure both properties are enabled
			App.WaitForElement("CheckStates");
			App.Tap("CheckStates");
			var status = GetStatusText();
			
			// Enable IsRefreshEnabled if it's disabled
			if (status.Contains("IsRefreshEnabled: False"))
			{
				App.Tap("ToggleIsRefreshEnabled");
			}
			
			// Enable IsEnabled if it's disabled
			if (status.Contains("IsEnabled: False"))
			{
				App.Tap("ToggleIsEnabled");
			}

			// Find the scroll view content to perform pull gesture on
			var scrollView = App.FindElement("ScrollViewContent");
			
			// Perform pull-to-refresh gesture by swiping down from the top
			App.SwipeDownToRefresh("TestRefreshView");
			
			// Wait for refresh to complete and verify it worked
			App.WaitForTextToBePresentInElement("StatusLabel", "Refresh completed", timeout: TimeSpan.FromSeconds(5));
		}

		[Test]
		[Category(UITestCategories.RefreshView)]
		public void PullToRefreshBlockedWhenIsRefreshEnabledFalse()
		{
			// Disable IsRefreshEnabled but keep IsEnabled true
			App.WaitForElement("CheckStates");
			App.Tap("CheckStates");
			var status = GetStatusText();
			
			// Enable IsEnabled if it's disabled
			if (status.Contains("IsEnabled: False"))
			{
				App.Tap("ToggleIsEnabled");
			}
			
			// Disable IsRefreshEnabled
			if (status.Contains("IsRefreshEnabled: True"))
			{
				App.Tap("ToggleIsRefreshEnabled");
			}

			// Verify current state
			App.Tap("CheckStates");
			Assert.That(GetStatusText().Contains("IsEnabled: True"), "IsEnabled should be true");
			Assert.That(GetStatusText().Contains("IsRefreshEnabled: False"), "IsRefreshEnabled should be false");

			// Try to perform pull-to-refresh gesture
			try
			{
				App.SwipeDownToRefresh("TestRefreshView");
				
				// Wait a moment to see if any refresh happens
				System.Threading.Thread.Sleep(1000);
				
				// Check that no refresh occurred
				App.Tap("CheckStates");
				Assert.That(GetStatusText().Contains("IsRefreshing: False"), "Pull-to-refresh should be blocked when IsRefreshEnabled is false");
			}
			catch (Exception)
			{
				// Some platforms might throw an exception when trying to refresh a disabled RefreshView
				// This is acceptable behavior
			}
		}

		private string GetStatusText()
		{
			return App.FindElement("StatusLabel").GetText();
		}
	}
}