using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public static class ViewExtensions
	{
		static int s_apiLevel;

		public static void RemoveFromParent(this AView view)
		{
			if (view == null)
				return;
			if (view.Parent == null)
				return;
			((ViewGroup)view.Parent).RemoveView(view);
		}

		public static void SetBackground(this AView view, Drawable drawable)
		{
			if (s_apiLevel == 0)
				s_apiLevel = (int)Build.VERSION.SdkInt;

			if (s_apiLevel < 16)
			{
#pragma warning disable 618
				view.SetBackgroundDrawable(drawable);
#pragma warning restore 618
			}
			else
				view.Background = drawable;
		}

		public static void SetWindowBackground(this AView view)
		{
			Context context = view.Context;
			using (var background = new TypedValue())
			{
				if (context.Theme.ResolveAttribute(global::Android.Resource.Attribute.WindowBackground, background, true))
				{
					string type = context.Resources.GetResourceTypeName(background.ResourceId).ToLower();
					switch (type)
					{
						case "color":
#pragma warning disable 618
							global::Android.Graphics.Color color = context.Resources.GetColor(background.ResourceId);
#pragma warning restore 618
							view.SetBackgroundColor(color);
							break;
						case "drawable":
#pragma warning disable 618
							using (Drawable drawable = context.Resources.GetDrawable(background.ResourceId))
#pragma warning restore 618

#pragma warning disable 618
								view.SetBackgroundDrawable(drawable);
#pragma warning restore 618
							break;
					}
				}
			}
		}
	}
}