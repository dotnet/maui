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
				throw new ArgumentNullException("serviceProvider");
			if (Key == null)
			{
				var lineInfoProvider = serviceProvider.GetService(typeof (IXmlLineInfoProvider)) as IXmlLineInfoProvider;
				var lineInfo = (lineInfoProvider != null) ? lineInfoProvider.XmlLineInfo : new XmlLineInfo();
				throw new XamlParseException("you must specify a key in {StaticResource}", lineInfo);
			}
			var valueProvider = serviceProvider.GetService(typeof (IProvideValueTarget)) as IProvideParentValues;
			if (valueProvider == null)
				throw new ArgumentException();
			var xmlLineInfoProvider = serviceProvider.GetService(typeof (IXmlLineInfoProvider)) as IXmlLineInfoProvider;
			var xmlLineInfo = xmlLineInfoProvider != null ? xmlLineInfoProvider.XmlLineInfo : null;

			foreach (var p in valueProvider.ParentObjects)
			{
				var ve = p as VisualElement;
				var resDict = ve?.Resources ?? p as ResourceDictionary;
				if (resDict == null)
					continue;
				object res;
				if (resDict.TryGetValue(Key, out res))
				{
					return ConvertCompiledOnPlatform(res);
				}
			}
			if (Application.Current != null && Application.Current.Resources != null &&
			    Application.Current.Resources.ContainsKey(Key))
			{
				var resource = Application.Current.Resources[Key];

				return ConvertCompiledOnPlatform(resource);
			}

			throw new XamlParseException($"StaticResource not found for key {Key}", xmlLineInfo);
		}

		static object ConvertCompiledOnPlatform(object resource)
		{
			var actualType = resource.GetType();
			if (actualType.GetTypeInfo().IsGenericType && actualType.GetGenericTypeDefinition() == typeof(OnPlatform<>))
			{
				// If we're accessing OnPlatform via a StaticResource in compiled XAML 
				// we'll have to handle the cast to the target type manually 
				// (Normally the compiled XAML handles this by calling `implicit` explicitly,
				// but it doesn't know to do that when it's using a static resource)
				var method = actualType.GetRuntimeMethod("op_Implicit", new[] { actualType });
				resource = method.Invoke(resource, new[] { resource });
			}

			return resource;
		}
	}
}