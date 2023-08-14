using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static partial class SoftInputExtensions
	{
		internal static bool HideSoftInput(this UIView inputView) => inputView.ResignFirstResponder();

		internal static bool ShowSoftInput(this UIView inputView) => inputView.BecomeFirstResponder();

		internal static bool IsSoftInputShowing(this UIView inputView) => inputView.IsFirstResponder;
	}
}