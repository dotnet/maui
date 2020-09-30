
using Android.Content;
using Android.Views;

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
