#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SearchBarTests
	{
		static AutoSuggestBox GetPlatformControl(SearchBarHandler handler) =>
			handler.PlatformView;

		static Task<string> GetPlatformText(SearchBarHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}

		[Theory]
		[InlineData(TextTransform.Uppercase, "AB")]
		[InlineData(TextTransform.Lowercase, "ab")]
		public async Task HandlerEditPreservesQueryCasingWhileMappingDisplay(TextTransform transform, string displayText)
		{
			const string query = "aB";
			var source = new Entry();
			var searchBar = new SearchBar { TextTransform = transform };
			searchBar.SetBinding(InputView.TextProperty, new Binding(nameof(Entry.Text), source: source));

			await AttachAndRun<SearchBarHandler>(searchBar, async handler =>
			{
				var platformSearchBar = GetPlatformControl(handler);
				var changes = new List<string>();
				searchBar.TextChanged += (_, args) => changes.Add(args.NewTextValue);

				// Pass the query through the same virtual-view setter as the WinUI user-input callback.
				handler.VirtualView.Text = query;

				var idle = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
				Assert.True(platformSearchBar.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () => idle.TrySetResult()));
				await idle.Task.WaitAsync(TimeSpan.FromSeconds(5));

				Assert.Equal(query, searchBar.Text);
				Assert.Equal(query, source.Text);
				Assert.Equal(query, Assert.Single(changes));
				Assert.Equal(displayText, platformSearchBar.Text);
			});
		}

		static int GetPlatformSelectionLength(SearchBarHandler searchBarHandler)
		{
			var platformSearchBar = GetPlatformControl(searchBarHandler);

			var textBox = platformSearchBar.GetFirstDescendant<TextBox>();

			if (textBox is not null)
			{
				return textBox.SelectionLength;
			}

			return -1;
		}

		static int GetPlatformCursorPosition(SearchBarHandler searchBarHandler)
		{
			var platformSearchBar = GetPlatformControl(searchBarHandler);

			var textBox = platformSearchBar.GetFirstDescendant<TextBox>();

			if (textBox is not null)
			{
				return textBox.SelectionStart;
			}

			return -1;
		}

		Task<float> GetPlatformOpacity(SearchBarHandler searchBarHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformControl(searchBarHandler);
				return (float)nativeView.Opacity;
			});
		}

		[Fact]
		[Description("The IsEnabled of a SearchBar should match with native IsEnabled")]
		public async Task VerifySearchBarIsEnabledProperty()
		{
			var searchBar = new SearchBar
			{
				IsEnabled = false
			};
			var expectedValue = searchBar.IsEnabled;

			var handler = await CreateHandlerAsync<SearchBarHandler>(searchBar);
			var nativeView = GetPlatformControl(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				var isEnabled = nativeView.IsEnabled;
				Assert.Equal(expectedValue, isEnabled);
			});
		}

		Task<bool> GetPlatformIsVisible(SearchBarHandler searchBarHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformControl(searchBarHandler);
				return nativeView.Visibility == Microsoft.UI.Xaml.Visibility.Visible;
			});
		}
	}
}
