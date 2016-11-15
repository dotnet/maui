using System;
using System.Linq.Expressions;

namespace Xamarin.Forms
{
	public static class BindableObjectExtensions
	{
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

		[Obsolete]
		public static void SetBinding<TSource>(this BindableObject self, BindableProperty targetProperty, Expression<Func<TSource, object>> sourceProperty, BindingMode mode = BindingMode.Default,
											   IValueConverter converter = null, string stringFormat = null)
		{
			if (self == null)
				throw new ArgumentNullException("self");
			if (targetProperty == null)
				throw new ArgumentNullException("targetProperty");
			if (sourceProperty == null)
				throw new ArgumentNullException("sourceProperty");

			Binding binding = Binding.Create(sourceProperty, mode, converter, stringFormat: stringFormat);
			self.SetBinding(targetProperty, binding);
		}
	}
}