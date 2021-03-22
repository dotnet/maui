using System.Collections.Generic;
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;

namespace Microsoft.Maui
{
	public static class ViewGroupExtensions
	{
		public static IEnumerable<T> GetChildrenOfType<T>(this AViewGroup self) where T : AView
		{
			for (var i = 0; i < self.ChildCount; i++)
			{
				AView? child = self.GetChildAt(i);

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
	}
}