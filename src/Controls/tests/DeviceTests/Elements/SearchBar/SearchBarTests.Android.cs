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
	}
}
