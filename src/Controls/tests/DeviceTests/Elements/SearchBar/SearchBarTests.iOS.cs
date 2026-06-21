using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SearchBarTests
	{
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
