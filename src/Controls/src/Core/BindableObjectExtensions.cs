using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/BindableObjectExtensions.xml" path="Type[@FullName='Microsoft.Maui.Controls.BindableObjectExtensions']/Docs/*" />
	public static class BindableObjectExtensions
	{
		internal static void PropagateBindingContext<T>(this BindableObject self, IEnumerable<T> children)
		{
			PropagateBindingContext(self, children, BindableObject.SetInheritedBindingContext);
		}

		internal static void PropagateBindingContext<T>(this BindableObject self, IEnumerable<T> children, Action<BindableObject, object> setChildBindingContext)
		{
			if (children == null)
				return;

			var bc = self.BindingContext;

			foreach (var child in children)
			{
				var bo = child as BindableObject;
				if (bo == null)
					continue;

				setChildBindingContext(bo, bc);
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableObjectExtensions.xml" path="//Member[@MemberName='SetBinding']/Docs/*" />
		public static void SetBinding(this BindableObject self, BindableProperty targetProperty, string path, BindingMode mode = BindingMode.Default, IValueConverter converter = null,
									  string stringFormat = null)
		{
			if (self == null)
				throw new ArgumentNullException("self");
			if (targetProperty == null)
				throw new ArgumentNullException("targetProperty");

			var binding = new Binding(path, mode, converter, stringFormat: stringFormat);
			self.SetBinding(targetProperty, binding);
		}

		public static T GetPropertyIfSet<T>(this BindableObject bindableObject, BindableProperty bindableProperty, T returnIfNotSet)
		{
			if (bindableObject == null)
				return returnIfNotSet;

			if (bindableObject.IsSet(bindableProperty))
				return (T)bindableObject.GetValue(bindableProperty);

			return returnIfNotSet;
		}

		public static void SetAppTheme<T>(this BindableObject self, BindableProperty targetProperty, T light, T dark) => self.SetBinding(targetProperty, new AppThemeBinding { Light = light, Dark = dark });

		/// <include file="../../docs/Microsoft.Maui.Controls/BindableObjectExtensions.xml" path="//Member[@MemberName='SetAppThemeColor']/Docs/*" />
		public static void SetAppThemeColor(this BindableObject self, BindableProperty targetProperty, Color light, Color dark) => SetAppTheme(self, targetProperty, light, dark);
	}
}
