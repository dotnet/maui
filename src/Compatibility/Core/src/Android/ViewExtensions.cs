using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using AndroidX.Core.Content;
using AColor = Android.Graphics.Color;
using ARect = Android.Graphics.Rect;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal static class ViewExtensions
	{
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
