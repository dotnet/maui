using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace System.Maui.Platform
{
	internal static class FrameworkElementExtensions
	{
		public static object UpdateDependencyColor(this DependencyObject depo, DependencyProperty dp, Color newColor)
		{
			if (!newColor.IsDefault)
				depo.SetValue(dp, newColor.ToBrush());
			else
				depo.ClearValue(dp);

			return depo.GetValue(dp);
		}

		internal static IEnumerable<T> GetChildren<T>(this DependencyObject parent) where T : DependencyObject
		{
			int myChildrenCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < myChildrenCount; i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				if (child is T)
					yield return child as T;
				else
				{
					foreach (var subChild in child.GetChildren<T>())
						yield return subChild;
				}
			}
		}
	}
}
