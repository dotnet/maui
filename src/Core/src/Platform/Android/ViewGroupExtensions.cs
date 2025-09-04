using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;

namespace Microsoft.Maui.Platform
{
	public static class ViewGroupExtensions
	{
		public static IEnumerable<T> GetChildrenOfType<T>(this AViewGroup viewGroup) where T : AView
		{
			for (var i = 0; i < viewGroup.ChildCount; i++)
			{
				AView? child = viewGroup.GetChildAt(i);

				if (child is T typedChild)
					yield return typedChild;

				if (child is AViewGroup)
				{
					IEnumerable<T>? myChildren = (child as AViewGroup)?.GetChildrenOfType<T>();
					if (myChildren != null)
						foreach (T nextChild in myChildren)
							yield return nextChild;
				}
			}
		}

		public static T? GetFirstChildOfType<T>(this AViewGroup viewGroup) where T : AView
		{
			for (var i = 0; i < viewGroup.ChildCount; i++)
			{
				AView? child = viewGroup.GetChildAt(i);

				if (child is T typedChild)
					return typedChild;

				if (child is AViewGroup vg)
				{
					var descendant = vg.GetFirstChildOfType<T>();
					if (descendant != null)
					{
						return descendant;
					}
				}
			}

			return null;
		}

		public static bool TryGetFirstChildOfType<T>(this AViewGroup viewGroup, [NotNullWhen(true)] out T? result) where T : AView
		{
			result = viewGroup.GetFirstChildOfType<T>();
			return result is not null;
		}

		internal static T? GetChildAt<T>(this AView view, int index) where T : AView
		{
			if (view is AViewGroup viewGroup && index < viewGroup.ChildCount)
				return (T?)viewGroup.GetChildAt(index);

			return null;
		}
	}
}
