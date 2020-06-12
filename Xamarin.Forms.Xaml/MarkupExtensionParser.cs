using System;
using System.Reflection;

namespace Xamarin.Forms.Xaml
{
	internal sealed class MarkupExtensionParser : MarkupExpressionParser, IExpressionParser<object>
	{
		IMarkupExtension markupExtension;

		public object Parse(string match, ref string remaining, IServiceProvider serviceProvider)
		{
			var typeResolver = serviceProvider.GetService(typeof (IXamlTypeResolver)) as IXamlTypeResolver;

			//shortcut for Binding and StaticResource, to avoid too many reflection calls.
			if (match == "Binding")
				markupExtension = new BindingExtension();
			else if (match == "TemplateBinding")
				markupExtension = new TemplateBindingExtension();
			else if (match == "StaticResource")
				markupExtension = new StaticResourceExtension();
			else if (match == "OnPlatform")
				markupExtension = new OnPlatformExtension();
			else if (match == "OnIdiom")
				markupExtension = new OnIdiomExtension();
			else if (match == "AppThemeBinding")
				markupExtension = new AppThemeBindingExtension();
			else if (match == "DataTemplate")
				markupExtension = new DataTemplateExtension();
			else
			{
				if (typeResolver == null)
					return null;
				Type type;

				//The order of lookup is to look for the Extension-suffixed class name first and then look for the class name without the Extension suffix.
				if (!typeResolver.TryResolve(match + "Extension", out type) && !typeResolver.TryResolve(match, out type))
					throw new XamlParseException($"MarkupExtension not found for {match}", serviceProvider);
				markupExtension = Activator.CreateInstance(type) as IMarkupExtension;
			}

			if (markupExtension == null)
				throw new XamlParseException($"Missing public default constructor for MarkupExtension {match}", serviceProvider);

			if (remaining == "}")
				return markupExtension.ProvideValue(serviceProvider);

			Property value;
			do {
				value = ParseProperty(serviceProvider, ref remaining);
				SetPropertyValue(value.name, value.strValue, value.value, serviceProvider);
			}
			while (!value.last);

			return markupExtension.ProvideValue(serviceProvider);
		}

		private void SetPropertyValue(string prop, string strValue, object value, IServiceProvider serviceProvider)
		{
			MethodInfo setter;
			if (prop == null) {
				//implicit property
				var t = markupExtension.GetType();
				prop = ApplyPropertiesVisitor.GetContentPropertyName(t.GetTypeInfo());
				if (prop == null)
					return;
				try {
					setter = t.GetRuntimeProperty(prop).SetMethod;
				}
				catch (AmbiguousMatchException e) {
					throw new XamlParseException($"Multiple properties with name  '{t}.{prop}' found.", serviceProvider, innerException: e);
				}
			}
			else {
				try {
					setter = markupExtension.GetType().GetRuntimeProperty(prop).SetMethod;
				}
				catch (AmbiguousMatchException e) {
					throw new XamlParseException($"Multiple properties with name  '{markupExtension.GetType()}.{prop}' found.", serviceProvider, innerException: e);
				}

			}
			if (value == null && strValue != null) {
				try {
					value = strValue.ConvertTo(markupExtension.GetType().GetRuntimeProperty(prop).PropertyType,
						(Func<TypeConverter>)null, serviceProvider, out Exception converterException);
					if (converterException != null)
						throw converterException;
				}
				catch (AmbiguousMatchException e) {
					throw new XamlParseException($"Multiple properties with name  '{markupExtension.GetType()}.{prop}' found.", serviceProvider, innerException: e);
				}
			}

			setter.Invoke(markupExtension, new[] { value });
		}
	}
}