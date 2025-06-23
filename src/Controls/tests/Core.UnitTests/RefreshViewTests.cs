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
		public void CanExecuteDisablesRefreshView()
		{
			RefreshView refreshView = new RefreshView();
			refreshView.Command = new Command(() => { }, () => false);
			Assert.False(refreshView.IsEnabled);
		}

		[Fact]
		public void CanExecuteEnablesRefreshView()
		{
			RefreshView refreshView = new RefreshView();
			refreshView.Command = new Command(() => { }, () => true);
			Assert.True(refreshView.IsEnabled);
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
		public void IsEnabledShouldCoerceCanExecute()
		{
			RefreshView refreshView = new RefreshView()
			{
				IsEnabled = false,
				Command = new Command(() => { })
			};
			Assert.False(refreshView.IsEnabled);
		}

		[Fact]
		public void CanExecuteChangesEnabled()
		{
			RefreshView refreshView = new RefreshView();

			bool canExecute = true;
			var command = new Command(() => { }, () => canExecute);
			refreshView.Command = command;

			canExecute = false;
			command.ChangeCanExecute();
			Assert.False(refreshView.IsEnabled);


			canExecute = true;
			command.ChangeCanExecute();
			Assert.True(refreshView.IsEnabled);
		}

		[Fact]
		public void CommandPropertyChangesEnabled()
		{
			RefreshView refreshView = new RefreshView();

			bool canExecute = true;
			var command = new Command((p) => { }, (p) => p != null && (bool)p);
			refreshView.CommandParameter = true;
			refreshView.Command = command;

			Assert.True(refreshView.IsEnabled);
			refreshView.CommandParameter = false;
			Assert.False(refreshView.IsEnabled);
			refreshView.CommandParameter = true;
			Assert.True(refreshView.IsEnabled);
		}

		[Fact]
		public void RemovedCommandEnablesRefreshView()
		{
			RefreshView refreshView = new RefreshView();

			bool canExecute = true;
			var command = new Command(() => { }, () => false);
			refreshView.Command = command;
			Assert.False(refreshView.IsEnabled);
			refreshView.Command = null;
			Assert.True(refreshView.IsEnabled);
			refreshView.Command = command;
			Assert.False(refreshView.IsEnabled);
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
	}
}
