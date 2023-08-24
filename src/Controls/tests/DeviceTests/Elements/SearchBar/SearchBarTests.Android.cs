using System.Linq;
using System.Threading.Tasks;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SearchBarTests
	{
		static SearchView GetPlatformControl(SearchBarHandler handler) =>
			handler.PlatformView;

		static EditText GetPlatformQueryEditor(SearchBarHandler handler) =>
			handler.QueryEditor;

		static Task<string> GetPlatformText(SearchBarHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Query);
		}

		static int GetPlatformSelectionLength(SearchBarHandler searchBarHandler)
		{
			var control = GetPlatformControl(searchBarHandler);
			var editText = control.GetChildrenOfType<EditText>().FirstOrDefault();
			return editText.SelectionEnd - editText.SelectionStart;
		}

		static int GetPlatformCursorPosition(SearchBarHandler searchBarHandler)
		{
			var control = GetPlatformControl(searchBarHandler);
			var editText = control.GetChildrenOfType<EditText>().FirstOrDefault();
			return editText.SelectionStart;
		}

		[Fact(DisplayName = "Horizontal text aligned when RTL is not supported")]
		public async Task HorizontalTextAlignedWhenRtlIsFalse()
		{
			if (Rtl.IsSupported)
				return;

			var searchBar = new SearchBar { Text = "Foo", HorizontalTextAlignment = TextAlignment.Center };

			var handler = await CreateHandlerAsync<SearchBarHandler>(searchBar);
			var platformSearchBarEditor = GetPlatformQueryEditor(handler);

			Assert.False(platformSearchBarEditor.Gravity.HasFlag(GravityFlags.Start), "Search bar editor should not have the Start flag.");
			Assert.False(platformSearchBarEditor.Gravity.HasFlag(GravityFlags.End), "Search bar editor should not have the End flag.");
			Assert.True(platformSearchBarEditor.Gravity.HasFlag(GravityFlags.CenterHorizontal), "Search bar editor should have the CenterHorizontal flag.");
		}

		[Fact(DisplayName = "Vertical text aligned when RTL is not supported")]
		public async Task VerticalTextAlignedWhenRtlIsFalse()
		{
			if (Rtl.IsSupported)
				return;

			var searchBar = new SearchBar { Text = "Foo", VerticalTextAlignment = TextAlignment.Center };

			var handler = await CreateHandlerAsync<SearchBarHandler>(searchBar);
			var platformSearchBarEditor = GetPlatformQueryEditor(handler);

			Assert.False(platformSearchBarEditor.Gravity.HasFlag(GravityFlags.Top), "Search bar editor should not have the Top flag.");
			Assert.False(platformSearchBarEditor.Gravity.HasFlag(GravityFlags.Bottom), "Search bar editor should not have the Bottom flag.");
			Assert.True(platformSearchBarEditor.Gravity.HasFlag(GravityFlags.CenterVertical), "Search bar editor should only have the CenterVertical flag.");
		}
	}
}
