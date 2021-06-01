using System.Collections.Generic;
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;

namespace Microsoft.Maui
{
	public static class ViewGroupExtensions
	{
		public static void AddView(this AViewGroup self, IView view, IMauiContext mauiContext)
		{
			var nativeView = view.ToNative(mauiContext);
			nativeView.RemoveFromParent();
			self.AddView(nativeView);
		}

		public static void InsertView(this AViewGroup self, IView view, int index, IMauiContext mauiContext)
		{
			var nativeView = view.ToNative(mauiContext);
			nativeView.RemoveFromParent();
			self.AddView(nativeView, index);
		}

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