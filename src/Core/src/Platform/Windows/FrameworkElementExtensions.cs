#nullable enable
using System;
using System.Linq;
using Microsoft.UI.Xaml;
using System.Collections.Concurrent;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.UI.Xaml.Controls;
using WBinding = Microsoft.UI.Xaml.Data.Binding;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WBindingExpression = Microsoft.UI.Xaml.Data.BindingExpression;

namespace Microsoft.Maui
{
	internal static class FrameworkElementExtensions
	{
		static readonly Lazy<ConcurrentDictionary<Type, DependencyProperty>> ForegroundProperties =
			new Lazy<ConcurrentDictionary<Type, DependencyProperty>>(() => new ConcurrentDictionary<Type, DependencyProperty>());

		public static WBrush GetForeground(this FrameworkElement element)
		{
			if (element == null)
				throw new ArgumentNullException(nameof(element));

			return (WBrush)element.GetValue(GetForegroundProperty(element));
		}

		public static WBinding? GetForegroundBinding(this FrameworkElement element)
		{
			WBindingExpression expr = element.GetBindingExpression(GetForegroundProperty(element));

			if (expr == null)
				return null;

			return expr.ParentBinding;
		}

		public static object GetForegroundCache(this FrameworkElement element)
		{
			WBinding? binding = GetForegroundBinding(element);

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
				throw new ArgumentNullException(nameof(element));

			element.SetValue(GetForegroundProperty(element), foregroundBrush);
		}

		public static void SetForeground(this FrameworkElement element, WBinding binding)
		{
			if (element == null)
				throw new ArgumentNullException(nameof(element));

			element.SetBinding(GetForegroundProperty(element), binding);
		}

		internal static IEnumerable<T?> GetDescendantsByName<T>(this DependencyObject parent, string elementName) where T : DependencyObject
		{
			int myChildrenCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < myChildrenCount; i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				var controlName = child.GetValue(FrameworkElement.NameProperty) as string;
				if (controlName == elementName && child is T t)
					yield return t;
				else
				{
					foreach (var subChild in child.GetDescendantsByName<T>(elementName))
						yield return subChild;
				}
			}
		}

		internal static T? GetFirstDescendant<T>(this DependencyObject element) where T : FrameworkElement
		{
			int count = VisualTreeHelper.GetChildrenCount(element);
			for (var i = 0; i < count; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(element, i);

				if ((child as T ?? GetFirstDescendant<T>(child)) is T target)
					return target;
			}

			return null;
		}

		static DependencyProperty? GetForegroundProperty(FrameworkElement element)
		{
			if (element is Control)
				return Control.ForegroundProperty;
			if (element is TextBlock)
				return TextBlock.ForegroundProperty;

			Type type = element.GetType();

			if (!ForegroundProperties.Value.TryGetValue(type, out var foregroundProperty))
			{
				if (ReflectionExtensions.GetFields(type).FirstOrDefault(f => f.Name == "ForegroundProperty") is not FieldInfo field)
					throw new ArgumentException("type is not a Foregroundable type");

				if (field.GetValue(null) is DependencyProperty property)
					ForegroundProperties.Value.TryAdd(type, property);

				return null;
			}

			return foregroundProperty;
		}

		internal static IEnumerable<T?> GetChildren<T>(this DependencyObject parent) where T : DependencyObject
		{
			int myChildrenCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < myChildrenCount; i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				if (child is T t)
					yield return t;
				else
				{
					foreach (var subChild in child.GetChildren<T>())
						yield return subChild;
				}
			}
		}
	}
}
