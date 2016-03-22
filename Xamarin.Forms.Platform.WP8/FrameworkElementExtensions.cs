using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xamarin.Forms.Platform.WinPhone
{
	internal static class FrameworkElementExtensions
	{
		static readonly Lazy<ConcurrentDictionary<Type, DependencyProperty>> ForegroundProperties =
			new Lazy<ConcurrentDictionary<Type, DependencyProperty>>(() => new ConcurrentDictionary<Type, DependencyProperty>());

		public static Brush GetForeground(this FrameworkElement element)
		{
			if (element == null)
				throw new ArgumentNullException("element");

			return (Brush)element.GetValue(GetForegroundProperty(element));
		}

		public static System.Windows.Data.Binding GetForegroundBinding(this FrameworkElement element)
		{
			System.Windows.Data.BindingExpression expr = element.GetBindingExpression(GetForegroundProperty(element));
			if (expr == null)
				return null;

			return expr.ParentBinding;
		}

		public static void SetForeground(this FrameworkElement element, Brush foregroundBrush)
		{
			if (element == null)
				throw new ArgumentNullException("element");

			element.SetValue(GetForegroundProperty(element), foregroundBrush);
		}

		public static void SetForeground(this FrameworkElement element, System.Windows.Data.Binding binding)
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
				FieldInfo field = type.GetFields(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(f => f.Name == "ForegroundProperty");
				if (field == null)
					throw new ArgumentException("type is not a Foregroundable type");

				var property = (DependencyProperty)field.GetValue(null);
				ForegroundProperties.Value.TryAdd(type, property);

				return property;
			}

			return foregroundProperty;
		}
	}
}