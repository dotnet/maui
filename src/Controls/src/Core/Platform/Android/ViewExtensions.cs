using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using AndroidX.Core.Content;
using Microsoft.Maui.Handlers;
using AColor = Android.Graphics.Color;
using ARect = Android.Graphics.Rect;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	public static class ViewExtensions
	{
		public static void SetBackground(this AView view, Drawable drawable)
		{

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
			if (!view.IsAlive())
			{
				return;
			}

			if (view.Id == AView.NoId)
			{
				view.Id = global::Android.Views.View.GenerateViewId();
			}
		}

		public static bool GetClipToOutline(this AView view)
		{
			if (!view.IsAlive())
				return false;

			return view.ClipToOutline;
		}

		public static void SetClipToOutline(this AView view, bool value)
		{
			if (!view.IsAlive())
				return;

			view.ClipToOutline = value;
		}

		public static bool SetElevation(this AView view, float value)
		{
			if (!view.IsAlive())
				return false;

			view.Elevation = value;
			return true;
		}

		internal static void MaybeRequestLayout(this AView view)
		{
			var isInLayout = false;
			if ((int)Build.VERSION.SdkInt >= 18)
				isInLayout = view.IsInLayout;

			if (!isInLayout && !view.IsLayoutRequested)
				view.RequestLayout();
		}

		internal static T GetParentOfType<T>(this IViewParent view)
			where T : class
		{
			if (view is T t)
				return t;

			while (view != null)
			{
				T parent = view.Parent as T;
				if (parent != null)
					return parent;

				view = view.Parent;
			}

			return default(T);
		}

		internal static T GetParentOfType<T>(this AView view)
			where T : class
		{
			T t = view as T;
			if (view != null)
				return t;

			return view.Parent.GetParentOfType<T>();
		}

		internal static T FindParentOfType<T>(this VisualElement element)
		{
			var navPage = element.GetParentsPath()
				.OfType<T>()
				.FirstOrDefault();
			return navPage;
		}

		internal static IEnumerable<Element> GetParentsPath(this VisualElement self)
		{
			Element current = self;

			while (!Application.IsApplicationOrNull(current.RealParent))
			{
				current = current.RealParent;
				yield return current;
			}
		}
	}
}
