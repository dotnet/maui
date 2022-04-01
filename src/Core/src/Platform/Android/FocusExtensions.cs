using Android.Runtime;
using Android.Views;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	public static class FocusExtensions
	{
		public static AView? FocusSearch(this AView focused, [GeneratedEnum] FocusSearchDirection direction)
		{
			if (focused.Parent is not ViewGroup parent)
				return null;

			return FocusFinder.Instance?.FindNextFocus(parent, focused, direction);
		}
	}
}