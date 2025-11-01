using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.RefreshView)]
	public partial class RefreshViewTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<RefreshView, RefreshViewHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<Entry, EntryHandler>();
				});
			});
		}

		[Fact(DisplayName = "Setting the content of RefreshView removes previous platform view from visual tree")]
		public async Task ChangingRefreshViewContentRemovesPreviousContentsPlatformViewFromVisualTree()
		{
			SetupBuilder();

			var refreshView = new RefreshView();
			var initialContent = new Label();
			refreshView.Content = initialContent;

			await AttachAndRun(refreshView, async (handler) =>
			{
				var newContent = new Entry();
				Assert.NotNull(((initialContent as IView).Handler as IPlatformViewHandler).PlatformView.GetParent());
				refreshView.Content = newContent;
				Assert.Null(((initialContent as IView).Handler as IPlatformViewHandler).PlatformView.GetParent());
				await Task.Yield();
				Assert.NotNull(((newContent as IView).Handler as IPlatformViewHandler).PlatformView.GetParent());
			});
		}

		[Fact(DisplayName = "IsRefreshEnabled defaults to true")]
		public void IsRefreshEnabledDefaultsToTrue()
		{
			var refreshView = new RefreshView();
			Assert.True(refreshView.IsRefreshEnabled);
		}

		[Fact(DisplayName = "IsRefreshEnabled can be set to false")]
		public void IsRefreshEnabledCanBeSetToFalse()
		{
			var refreshView = new RefreshView();
			refreshView.IsRefreshEnabled = false;
			Assert.False(refreshView.IsRefreshEnabled);
		}

		[Fact(DisplayName = "IsRefreshEnabled prevents IsRefreshing from being set to true")]
		public void IsRefreshEnabledPreventsIsRefreshingFromBeingSetToTrue()
		{
			var refreshView = new RefreshView();
			refreshView.IsRefreshEnabled = false;
			refreshView.IsRefreshing = true;
			Assert.False(refreshView.IsRefreshing);
		}

		[Fact(DisplayName = "IsRefreshing can be set to false when IsRefreshEnabled is false")]
		public void IsRefreshingCanBeSetToFalseWhenIsRefreshEnabledIsFalse()
		{
			var refreshView = new RefreshView();
			refreshView.IsRefreshing = true;
			Assert.True(refreshView.IsRefreshing);

			refreshView.IsRefreshEnabled = false;
			Assert.False(refreshView.IsRefreshing); // Should be automatically cleared
		}

		[Fact(DisplayName = "IsEnabled prevents IsRefreshing from being set to true")]
		public void IsEnabledPreventsIsRefreshingFromBeingSetToTrue()
		{
			var refreshView = new RefreshView();
			refreshView.IsEnabled = false;
			refreshView.IsRefreshing = true;
			Assert.False(refreshView.IsRefreshing);
		}

		[Fact(DisplayName = "Setting IsRefreshEnabled to false while refreshing stops refresh")]
		public void SettingIsRefreshEnabledToFalseWhileRefreshingStopsRefresh()
		{
			var refreshView = new RefreshView();
			refreshView.IsRefreshing = true;
			Assert.True(refreshView.IsRefreshing);

			refreshView.IsRefreshEnabled = false;
			Assert.False(refreshView.IsRefreshing);
		}

		[Fact(DisplayName = "Setting IsEnabled to false while refreshing stops refresh")]
		public void SettingIsEnabledToFalseWhileRefreshingStopsRefresh()
		{
			var refreshView = new RefreshView();
			refreshView.IsRefreshing = true;
			Assert.True(refreshView.IsRefreshing);

			refreshView.IsEnabled = false;
			Assert.False(refreshView.IsRefreshing);
		}
	}
}
