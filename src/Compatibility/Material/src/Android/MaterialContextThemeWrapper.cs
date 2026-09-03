
using Android.Content;
using Android.Views;

namespace Microsoft.Maui.Controls.Compatibility.Material.Android
{
	public class MaterialContextThemeWrapper : ContextThemeWrapper
	{
		public MaterialContextThemeWrapper(Context context) : this(context, Resource.Style.Microsoft.Maui.ControlsFormsMaterialTheme)
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
