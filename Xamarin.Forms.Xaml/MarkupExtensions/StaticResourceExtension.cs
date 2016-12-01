using System;
using System.Reflection;

namespace Xamarin.Forms.Xaml
{
	[ContentProperty("Key")]
	public sealed class StaticResourceExtension : IMarkupExtension
	{
		public string Key { get; set; }

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			if (Key == null) {
				var lineInfoProvider = serviceProvider.GetService(typeof(IXmlLineInfoProvider)) as IXmlLineInfoProvider;
				var lineInfo = (lineInfoProvider != null) ? lineInfoProvider.XmlLineInfo : new XmlLineInfo();
				throw new XamlParseException("you must specify a key in {StaticResource}", lineInfo);
			}
			var valueProvider = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideParentValues;
			if (valueProvider == null)
				throw new ArgumentException();
			var xmlLineInfoProvider = serviceProvider.GetService(typeof(IXmlLineInfoProvider)) as IXmlLineInfoProvider;
			var xmlLineInfo = xmlLineInfoProvider != null ? xmlLineInfoProvider.XmlLineInfo : null;
			object resource = null;

			foreach (var p in valueProvider.ParentObjects) {
				var ve = p as VisualElement;
				var resDict = ve?.Resources ?? p as ResourceDictionary;
				if (resDict == null)
					continue;
				if (resDict.TryGetValue(Key, out resource))
					break;
			}
			if (resource == null && Application.Current != null && Application.Current.Resources != null &&
				Application.Current.Resources.ContainsKey(Key))
				resource = Application.Current.Resources[Key];

			if (resource == null)
				throw new XamlParseException($"StaticResource not found for key {Key}", xmlLineInfo);

			var bp = valueProvider.TargetProperty as BindableProperty;
			var pi = valueProvider.TargetProperty as PropertyInfo;
			var propertyType = bp?.ReturnType ?? pi?.PropertyType;
			if (propertyType == null) {
				if (resource.GetType().GetTypeInfo().IsGenericType && (resource.GetType().GetGenericTypeDefinition() == typeof(OnPlatform<>) || resource.GetType().GetGenericTypeDefinition() == typeof(OnIdiom<>))) {
					// This is only there to support our backward compat story with pre 2.3.3 compiled Xaml project who was not providing TargetProperty
					var method = resource.GetType().GetRuntimeMethod("op_Implicit", new[] { resource.GetType() });
					resource = method.Invoke(resource, new[] { resource });
				}
				return resource;
			}
			if (propertyType.IsAssignableFrom(resource.GetType()))
				return resource;
			var implicit_op = resource.GetType().GetRuntimeMethod("op_Implicit", new [] { resource.GetType() });
			if (implicit_op != null && propertyType.IsAssignableFrom(implicit_op.ReturnType))
				return implicit_op.Invoke(resource, new [] { resource });

			return resource;
		}
	}
}