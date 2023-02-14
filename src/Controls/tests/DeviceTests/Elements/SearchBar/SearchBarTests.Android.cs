using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SearchBarTests
	{
		SearchView GetPlatformControl(SearchBarHandler handler) =>
			handler.PlatformView;

		Task<string> GetPlatformText(SearchBarHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Query);
		}

		[Fact]
		public async Task ShowsKeyboardOnFocus()
		{
			var button = new Button();
			var searchBar = new SearchBar();
			var handler = await CreateHandlerAsync<SearchBarHandler>(searchBar);

			await InvokeOnMainThreadAsync(async () =>
			{
				await handler.PlatformView.AttachAndRun(async () =>
				{
					button.Focus();
					searchBar.Focus();
					await AssertionExtensions.WaitForKeyboardToShow(handler.PlatformView);
				});
			});
		}
	}
}
