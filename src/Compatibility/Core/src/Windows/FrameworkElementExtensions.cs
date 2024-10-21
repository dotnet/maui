using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls.Internals;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WBinding = Microsoft.UI.Xaml.Data.Binding;
using WBindingExpression = Microsoft.UI.Xaml.Data.BindingExpression;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	[PortHandler]
	internal static class FrameworkElementExtensions
	{
		static readonly Lazy<ConcurrentDictionary<Type, DependencyProperty>> ForegroundProperties =
			new Lazy<ConcurrentDictionary<Type, DependencyProperty>>(() => new ConcurrentDictionary<Type, DependencyProperty>());

		public static WBrush GetForeground(this FrameworkElement element)
		{
			if (element == null)
				throw new ArgumentNullException("element");

			return (WBrush)element.GetValue(GetForegroundProperty(element));
		}

		public static WBinding GetForegroundBinding(this FrameworkElement element)
		{
			WBindingExpression expr = element.GetBindingExpression(GetForegroundProperty(element));
			if (expr == null)
				return null;

			return expr.ParentBinding;
		}

		public static object GetForegroundCache(this FrameworkElement element)
		{
			WBinding binding = GetForegroundBinding(element);
			if (binding != null)
				return binding;

			return GetForeground(element);
		}

		public static void RestoreForegroundCache(this FrameworkElement element, object cache)
		{
			var binding = cache as WBinding;
			if (binding != null)
				SetForeground(element, binding);
			else
				SetForeground(element, (WBrush)cache);
		}

		public static void SetForeground(this FrameworkElement element, WBrush foregroundBrush)
		{
			if (element == null)
				throw new ArgumentNullException("element");

			element.SetValue(GetForegroundProperty(element), foregroundBrush);
		}

		public static void SetForeground(this FrameworkElement element, WBinding binding)
		{
			if (element == null)
				throw new ArgumentNullException("element");

			element.SetBinding(GetForegroundProperty(element), binding);
		}

		static DependencyProperty GetForegroundProperty(FrameworkElement element)
		{
			if (element is Control)
				return Control.ForegroundProperty;
			if (element is TextBlock)
				return TextBlock.ForegroundProperty;

			Type type = element.GetType();

			DependencyProperty foregroundProperty;
			if (!ForegroundProperties.Value.TryGetValue(type, out foregroundProperty))
			{
#pragma warning disable IL2075 // The Compatibility assembly is not trimmable
				FieldInfo field = type.GetField("ForegroundProperty", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
#pragma warning restore IL2075
				if (field == null)
					throw new ArgumentException("type is not a Foregroundable type");

				var property = (DependencyProperty)field.GetValue(null);
				ForegroundProperties.Value.TryAdd(type, property);

				return property;
			}

			return foregroundProperty;
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