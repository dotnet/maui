using Android.Content;
using Android.Support.V4.Widget;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android
{
	internal static class TextViewExtensions
	{
		public static void SetTextAppearanceCompat(this TextView textView, Context context, int resId)
		{
			TextViewCompat.SetTextAppearance(textView, resId);
		}
	}
}