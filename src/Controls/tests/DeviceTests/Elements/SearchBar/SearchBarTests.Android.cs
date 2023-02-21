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
			var searchBar = new SearchBar();

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<SearchBarHandler>(searchBar);

				await handler.PlatformView.AttachAndRun(async () =>
				{
					searchBar.Focus();
					await AssertionExtensions.WaitForKeyboardToShow(handler.QueryEditor);

					// Test that keyboard reappears when refocusing on an already focused TextInput control
					await AssertionExtensions.HideKeyboardForView(handler.QueryEditor);
					await AssertionExtensions.WaitForKeyboardToHide(handler.QueryEditor);
					searchBar.Focus();
					await AssertionExtensions.WaitForKeyboardToShow(handler.QueryEditor);
				});
			});
		}
	}
}
