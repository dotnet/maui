using Android.Content;
using Android.OS;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android
{
	internal static class TextViewExtensions
	{
		public static void SetTextAppearanceCompat(this TextView textView, Context context, int resId)
		{
			if ((int)Build.VERSION.SdkInt < 23)
			{
#pragma warning disable 618 // Using older version of SetTextAppearance for compatibility with API 15-22
				textView.SetTextAppearance(context, resId);
#pragma warning restore 618
			}
			else
				textView.SetTextAppearance(resId);
		}
	}
}