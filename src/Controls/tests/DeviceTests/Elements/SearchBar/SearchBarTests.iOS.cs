using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SearchBarTests
	{
		[Fact]
		public async Task CancelButtonIsHiddenWithoutFocusByDefault()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var searchBar = new Controls.SearchBar();
				var handler = CreateHandler<SearchBarHandler>(searchBar);

				Assert.False(handler.PlatformView.ShowsCancelButton);
				searchBar.Text = "search";
				Assert.False(handler.PlatformView.ShowsCancelButton);
				searchBar.Text = string.Empty;
				Assert.False(handler.PlatformView.ShowsCancelButton);
			});
		}

		[Collection(RunInNewWindowCollection)]
		[Category(TestCategory.SearchBar)]
		public class CancelButtonVisibilityTests : ControlsHandlerTestBase
		{
			[Theory]
			[InlineData(ClearButtonVisibility.WhileEditing)]
			[InlineData(ClearButtonVisibility.Never)]
			public async Task CancelButtonCanBeToggledWhileEditing(ClearButtonVisibility visibility)
			{
				await InvokeOnMainThreadAsync(async () =>
				{
					var searchBar = new Controls.SearchBar { Text = "initial", ClearButtonVisibility = visibility };
					var handler = CreateHandler<SearchBarHandler>(searchBar);

					await handler.PlatformView.AttachAndRun(async () =>
					{
						Assert.True(searchBar.Focus());
						await AssertEventually(() => searchBar.IsFocused);
						Assert.Equal(visibility == ClearButtonVisibility.WhileEditing, handler.PlatformView.ShowsCancelButton);

						searchBar.ClearButtonVisibility = ClearButtonVisibility.Never;
						Assert.False(handler.PlatformView.ShowsCancelButton);
						searchBar.CancelButtonColor = Graphics.Colors.Red;
						searchBar.Text = "changed";
						Assert.False(handler.PlatformView.ShowsCancelButton);

						handler.QueryEditor.Text = "typed";
						handler.QueryEditor.SendActionForControlEvents(UIControlEvent.EditingChanged);
						Assert.Equal("typed", searchBar.Text);
						Assert.False(handler.PlatformView.ShowsCancelButton);

						searchBar.ClearButtonVisibility = ClearButtonVisibility.WhileEditing;
						Assert.True(handler.PlatformView.ShowsCancelButton);
						Assert.True(searchBar.IsFocused);
						searchBar.Text = string.Empty;
						Assert.False(handler.PlatformView.ShowsCancelButton);
						searchBar.Text = null;
						Assert.False(handler.PlatformView.ShowsCancelButton);
						searchBar.Text = "again";
						Assert.True(handler.PlatformView.ShowsCancelButton);
						searchBar.Unfocus();
						await AssertEventually(() => !searchBar.IsFocused);
						Assert.False(handler.PlatformView.ShowsCancelButton);
						Assert.True(searchBar.Focus());
						await AssertEventually(() => searchBar.IsFocused);
						Assert.True(handler.PlatformView.ShowsCancelButton);
						searchBar.ClearButtonVisibility = ClearButtonVisibility.Never;
						Assert.False(handler.PlatformView.ShowsCancelButton);
					});
				});
			}
		}

		static MauiSearchBar GetPlatformControl(SearchBarHandler handler) =>
			handler.PlatformView;

		static Task<string> GetPlatformText(SearchBarHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}

		static Task<Graphics.Color> GetPlatformCancelButtonColor(SearchBarHandler handler) => InvokeOnMainThreadAsync(() =>
		{
			if (handler.PlatformView.TraitCollection.UserInterfaceIdiom == UIUserInterfaceIdiom.Mac)
				return GetPlatformControl(handler).FindDescendantView<UIButton>().TintColor.ToColor();

			return GetPlatformControl(handler).FindDescendantView<UIButton>().TitleColor(UIControlState.Normal).ToColor();
		});

		static int GetPlatformSelectionLength(SearchBarHandler searchBarHandler)
		{
			var control = searchBarHandler.QueryEditor;
			return control.GetSelectedTextLength();
		}

		static int GetPlatformCursorPosition(SearchBarHandler searchBarHandler)
		{
			var control = searchBarHandler.QueryEditor;
			return control.GetCursorPosition();
		}

		Task<float> GetPlatformOpacity(SearchBarHandler searchBarHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformControl(searchBarHandler);
				return (float)nativeView.Alpha;
			});
		}

		Task<bool> GetPlatformIsVisible(SearchBarHandler searchBarHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformControl(searchBarHandler);
				return !nativeView.Hidden;
			});
		}
	}
}
