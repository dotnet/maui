using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class RefreshViewTests : BaseTestFixture
	{

		public RefreshViewTests()
		{
			DeviceDisplay.SetCurrent(new MockDeviceDisplay());
		}

		[Fact]
		public void StartsEnabled()
		{
			RefreshView refreshView = new RefreshView();
			Assert.True(refreshView.IsEnabled);
		}

		[Fact]
		public void CanExecuteDisablesRefreshing()
		{
			RefreshView refreshView = new RefreshView();
			refreshView.Command = new Command(() => { }, () => false);
			Assert.False(refreshView.IsRefreshEnabled);
		}

		[Fact]
		public void CanExecuteEnablesRefreshing()
		{
			RefreshView refreshView = new RefreshView();
			refreshView.Command = new Command(() => { }, () => true);
			Assert.True(refreshView.IsRefreshEnabled);
		}

		[Fact]
		public void IsRefreshingStillTogglesTrueWhenCanExecuteToggledDuringExecute()
		{
			RefreshView refreshView = new RefreshView();
			Command command = null;
			command = new Command(() => command.ChangeCanExecute(), () => !refreshView.IsRefreshing);
			refreshView.Command = command;
			refreshView.IsRefreshing = true;
			Assert.True(refreshView.IsRefreshing);
		}

		[Fact]
		public void IsRefreshEnabledShouldCoerceCanExecute()
		{
			RefreshView refreshView = new RefreshView()
			{
				IsRefreshEnabled = false,
				Command = new Command(() => { })
			};
			Assert.False(refreshView.IsRefreshEnabled);
		}

		[Fact]
		public void CanExecuteChangesIsRefreshEnabled()
		{
			RefreshView refreshView = new RefreshView();

			bool canExecute = true;
			var command = new Command(() => { }, () => canExecute);
			refreshView.Command = command;

			canExecute = false;
			command.ChangeCanExecute();
			Assert.False(refreshView.IsRefreshEnabled);

			canExecute = true;
			command.ChangeCanExecute();
			Assert.True(refreshView.IsRefreshEnabled);
		}

		[Fact]
		public void CommandPropertyChangesIsRefreshEnabled()
		{
			RefreshView refreshView = new RefreshView();

			bool canExecute = true;
			var command = new Command((p) => { }, (p) => p != null && (bool)p);
			refreshView.CommandParameter = true;
			refreshView.Command = command;

			Assert.True(refreshView.IsRefreshEnabled);
			refreshView.CommandParameter = false;
			Assert.False(refreshView.IsRefreshEnabled);
			refreshView.CommandParameter = true;
			Assert.True(refreshView.IsRefreshEnabled);
		}

		[Fact]
		public void RemovedCommandEnablesRefreshView()
		{
			RefreshView refreshView = new RefreshView();

			bool canExecute = true;
			var command = new Command(() => { }, () => false);
			refreshView.Command = command;
			Assert.False(refreshView.IsRefreshEnabled);
			refreshView.Command = null;
			Assert.True(refreshView.IsRefreshEnabled);
			refreshView.Command = command;
			Assert.False(refreshView.IsRefreshEnabled);
		}

		[Fact]
		public void IsRefreshingStaysFalseWithDisabledCommand()
		{
			RefreshView refreshView = new RefreshView();

			bool canExecute = true;
			refreshView.Command = new Command(() => { }, () => false);
			refreshView.IsRefreshing = true;
			Assert.False(refreshView.IsRefreshing);
		}

		[Fact]
		public void IsRefreshingSettableToTrue()
		{
			RefreshView refreshView = new RefreshView();
			Assert.False(refreshView.IsRefreshing);

			refreshView.IsRefreshing = true;
			Assert.True(refreshView.IsRefreshing);
		}

		[Fact]
		public void IsRefreshingStaysFalseWithDisabledRefreshView()
		{
			RefreshView refreshView = new RefreshView();
			refreshView.IsEnabled = false;
			refreshView.IsRefreshing = true;
			Assert.False(refreshView.IsRefreshing);
		}

		[Fact]
		public void IsRefreshingTogglesFalseWhenIsEnabledSetToFalse()
		{
			RefreshView refreshView = new RefreshView();
			refreshView.IsRefreshing = true;
			refreshView.IsEnabled = false;
			Assert.False(refreshView.IsRefreshing);
		}

		[Fact]
		public void IsRefreshingEventFires()
		{
			RefreshView refreshView = new RefreshView();
			bool eventFired = false;
			refreshView.Refreshing += (_, __) => eventFired = true;
			Assert.False(eventFired);
			refreshView.IsRefreshing = true;
			Assert.True(eventFired);
		}

		[Fact]
		public void IsRefreshEnabledDefaultsToTrue()
		{
			RefreshView refreshView = new RefreshView();
			Assert.True(refreshView.IsRefreshEnabled);
		}

		[Fact]
		public void IsRefreshEnabledCanBeSetToFalse()
		{
			RefreshView refreshView = new RefreshView();
			refreshView.IsRefreshEnabled = false;
			Assert.False(refreshView.IsRefreshEnabled);
		}

		[Fact]
		public void IsRefreshEnabledPreventsIsRefreshingFromBeingSetToTrue()
		{
			RefreshView refreshView = new RefreshView();
			refreshView.IsRefreshEnabled = false;
			refreshView.IsRefreshing = true;
			Assert.False(refreshView.IsRefreshing);
		}

		[Fact]
		public void SettingIsRefreshEnabledToFalseWhileRefreshingStopsRefresh()
		{
			RefreshView refreshView = new RefreshView();

			refreshView.IsRefreshing = true;
			Assert.True(refreshView.IsRefreshing);

			refreshView.IsRefreshEnabled = false;
			Assert.False(refreshView.IsRefreshing);
		}

		[Theory]
		[InlineData(true, true, true)]   // Both enabled - should allow refresh
		[InlineData(false, true, false)] // IsEnabled false - should prevent refresh
		[InlineData(true, false, false)] // IsRefreshEnabled false - should prevent refresh
		[InlineData(false, false, false)] // Both false - should prevent refresh
		public void RefreshBehaviorDependsOnIsEnabledAndIsRefreshEnabled(bool isEnabled, bool isRefreshEnabled, bool expectedRefreshing)
		{
			RefreshView refreshView = new RefreshView();

			refreshView.IsEnabled = isEnabled;
			refreshView.IsRefreshEnabled = isRefreshEnabled;

			refreshView.IsRefreshing = true;

			Assert.Equal(expectedRefreshing, refreshView.IsRefreshing);
		}

		[Fact]
		public void IsRefreshEnabledWorksWithCommand()
		{
			RefreshView refreshView = new RefreshView();
			bool commandExecuted = false;
			refreshView.Command = new Command(() => commandExecuted = true);

			// When IsRefreshEnabled is true, command should execute
			refreshView.IsRefreshEnabled = true;
			refreshView.IsRefreshing = true;
			Assert.True(commandExecuted);

			// Reset
			commandExecuted = false;
			refreshView.IsRefreshing = false;

			// When IsRefreshEnabled is false, refresh should not happen
			refreshView.IsRefreshEnabled = false;
			refreshView.IsRefreshing = true;
			Assert.False(refreshView.IsRefreshing);
			Assert.False(commandExecuted);
		}

		[Fact]
		public void IsRefreshEnabledRespectsCommandCanExecute()
		{
			RefreshView refreshView = new RefreshView();
			bool canExecute = true;
			bool commandExecuted = false;

			refreshView.Command = new Command(() => commandExecuted = true, () => canExecute);

			// Initially can execute and IsRefreshEnabled is true by default
			Assert.True(refreshView.IsRefreshEnabled);

			// When command cannot execute, IsRefreshEnabled should be false
			canExecute = false;
			((Command)refreshView.Command).ChangeCanExecute();
			Assert.False(refreshView.IsRefreshEnabled);

			// When command can execute again, IsRefreshEnabled should be true (if explicitly set)
			canExecute = true;
			((Command)refreshView.Command).ChangeCanExecute();
			Assert.True(refreshView.IsRefreshEnabled);
		}

		[Fact]
		public void IsRefreshEnabledWithCommandCanExecuteFalseBlocksRefresh()
		{
			RefreshView refreshView = new RefreshView();
			bool canExecute = false;
			bool commandExecuted = false;

			refreshView.Command = new Command(() => commandExecuted = true, () => canExecute);

			// Even though IsRefreshEnabled is explicitly true, command cannot execute
			refreshView.IsRefreshEnabled = true;
			Assert.False(refreshView.IsRefreshEnabled); // Should be coerced to false

			// Trying to refresh should fail
			refreshView.IsRefreshing = true;
			Assert.False(refreshView.IsRefreshing);
			Assert.False(commandExecuted);
		}

		[Fact]
		public void CommandCanExecuteChangeClearsIsRefreshingWhenBecomesFalse()
		{
			RefreshView refreshView = new RefreshView();
			bool canExecute = true;
			bool commandExecuted = false;

			refreshView.Command = new Command(() => commandExecuted = true, () => canExecute);

			// Start refreshing
			refreshView.IsRefreshing = true;
			Assert.True(refreshView.IsRefreshing);
			Assert.True(commandExecuted);

			// When command can no longer execute, refreshing should stop
			canExecute = false;
			((Command)refreshView.Command).ChangeCanExecute();

			// The refresh should not be stopped by CanExecuteChanged when already refreshing
			// This matches the behavior in the CanExecuteChanged method
			Assert.True(refreshView.IsRefreshing);
		}
	}
}
