using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using AView = Android.Views.View;
using AColor = Android.Graphics.Color;

namespace Xamarin.Forms.Platform.Android
{
	public static class ViewExtensions
	{
		static readonly int s_apiLevel;

		static ViewExtensions()
		{
			s_apiLevel = (int)Build.VERSION.SdkInt;
		}

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
			if (s_apiLevel < 16)
			{
#pragma warning disable 618 // Using older method for compatibility with API 15
				view.SetBackgroundDrawable(drawable);
#pragma warning restore 618
			}
			else
			{
				view.Background = drawable;
			}
			
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
							var color = new AColor(ContextCompat.GetColor(context, background.ResourceId));
							view.SetBackgroundColor(color);
							break;
						case "drawable":
							using (Drawable drawable = ContextCompat.GetDrawable(context, background.ResourceId))
								view.SetBackground(drawable);
							break;
					}
				}
			}
		}

		public static void EnsureId(this AView view)
		{
			if (view.IsDisposed())
			{
				return;
			}

			if (view.Id == AView.NoId)
			{
				view.Id = Platform.GenerateViewId();
			}
		}
	}
}