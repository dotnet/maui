
using Android.Content;
using Android.Views;
using Xamarin.Forms.Platform.Android;
#if __ANDROID_29__
using AndroidAppCompat = AndroidX.AppCompat.Content.Res.AppCompatResources;
#else
using AndroidAppCompat = Android.Support.V7.Content.Res.AppCompatResources;
#endif

namespace Xamarin.Forms.Material.Android
{
	public class MaterialContextThemeWrapper : ContextThemeWrapper
	{
		public MaterialContextThemeWrapper(Context context) : this(context, Resource.Style.XamarinFormsMaterialTheme)
		{
		}

		MaterialContextThemeWrapper(Context context, int themeResId) : base(context, themeResId)
		{

		}

		public static MaterialContextThemeWrapper Create(Context context)
		{
			if (context is MaterialContextThemeWrapper materialContext)
				return materialContext;

			return new MaterialContextThemeWrapper(context);
		}
	}
}
