using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Microsoft.Maui.Controls.Xaml
{
	[ContentProperty(nameof(Key))]
	[RequireService([typeof(IXmlLineInfoProvider), typeof(IProvideParentValues), typeof(IRootObjectProvider)])]
	[ProvideCompiled("Microsoft.Maui.Controls.Build.Tasks.StaticResourceExtension")]
	public sealed class StaticResourceExtension : IMarkupExtension
	{
		public string Key { get; set; }

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			if (serviceProvider is null)
				throw new ArgumentNullException(nameof(serviceProvider));
			if (Key is null)
				throw new XamlParseException("you must specify a key in {StaticResource}", serviceProvider);
			if (serviceProvider.GetService(typeof(IProvideValueTarget)) is not IProvideParentValues valueProvider)
				throw new ArgumentException(null, nameof(serviceProvider));

			if (!TryGetResource(Key, valueProvider.ParentObjects.ToArray(), out var resource, out var resourceDictionary)
				&& !TryGetApplicationLevelResource(Key, out resource, out resourceDictionary))
			{
				var xmlLineInfo = serviceProvider.GetService(typeof(IXmlLineInfoProvider)) is IXmlLineInfoProvider xmlLineInfoProvider ? xmlLineInfoProvider.XmlLineInfo : null;
				if (Controls.Internals.ResourceLoader.ExceptionHandler2 is var ehandler && ehandler != null)
				{
					var ex = new XamlParseException($"StaticResource not found for key {Key}", xmlLineInfo);
					var rootObjectProvider = (IRootObjectProvider)serviceProvider.GetService(typeof(IRootObjectProvider));
					var root = rootObjectProvider.RootObject;
					ehandler.Invoke((ex, XamlFilePathAttribute.GetFilePathForObject(root)));
					return null;
				}
				else
					throw new XamlParseException($"StaticResource not found for key {Key}", xmlLineInfo);
			}

			Diagnostics.ResourceDictionaryDiagnostics.OnStaticResourceResolved(resourceDictionary, Key, valueProvider.TargetObject, valueProvider.TargetProperty);

// =======
// 			var lineInfo = serviceProvider.GetService(typeof(IXmlLineInfoProvider)) is IXmlLineInfoProvider xmlLineInfoProvider ? xmlLineInfoProvider.XmlLineInfo : null;
// 			var resource = GetResource(
// 				Key,
// 				valueProvider.ParentObjects.ToArray(),
// 				valueProvider.TargetObject,
// 				valueProvider.TargetProperty,
// 				lineInfo);
// >>>>>>> d3af29adeb (incremental, dependency first, sourcegen inflator)
			return CastTo(resource, valueProvider.TargetProperty);
		}

// #pragma warning disable RS0017
// 		public static object GetValue(
// #pragma warning restore RS0017
// 			string key,
// 			ReadOnlySpan<object> parentObjects,
// 			object targetObject,
// 			PropertyInfo targetProperty,
// 			IXmlLineInfo xmlLineInfo)
// 		{
// 			if (!TryGetResource(key, parentObjects, out var resource, out var resourceDictionary)
// 				&& !TryGetApplicationLevelResource(key, out resource, out resourceDictionary))
// 			{
// 				throw new XamlParseException($"StaticResource not found for key {key}", xmlLineInfo);
// 			}

// 			Diagnostics.ResourceDictionaryDiagnostics.OnStaticResourceResolved(resourceDictionary, key, targetObject, targetProperty);

// 			return CastTo(resource, targetProperty.PropertyType);
// 		}

// #pragma warning disable RS0017
// 		public static object GetValue(
// #pragma warning disable RS0017
// 			string key,
// 			ReadOnlySpan<object> parentObjects,
// 			object targetObject,
// 			BindableProperty targetProperty,
// 			IXmlLineInfo xmlLineInfo)
// 		{
// 			var resource = GetResource(key, parentObjects, targetObject, targetProperty, xmlLineInfo);
// 			return CastTo(resource, targetProperty.ReturnType);
// 		}

		//used by X.HR.F
		internal static object CastTo(object value, object targetProperty)
		{
			var bp = targetProperty as BindableProperty;
			var pi = targetProperty as PropertyInfo;
			return CastTo(value, bp?.ReturnType ?? pi?.PropertyType);
		}

		static object CastTo(object value, Type propertyType)
		{
			Type valueType = value.GetType();
			if (propertyType is null || propertyType.IsAssignableFrom(valueType))
				return value;

			if (TypeConversionHelper.TryConvert(value, propertyType, out var convertedValue))
			{
				return convertedValue;
			}

			return value;
		}

		static object GetResource(string key, ReadOnlySpan<object> parentObjects, object targetObject, object targetProperty, IXmlLineInfo xmlLineInfo)
		{
			if (!TryGetResource(key, parentObjects, out var resource, out var resourceDictionary)
				&& !TryGetApplicationLevelResource(key, out resource, out resourceDictionary))
			{
				throw new XamlParseException($"StaticResource not found for key {key}", xmlLineInfo);
			}

			Diagnostics.ResourceDictionaryDiagnostics.OnStaticResourceResolved(resourceDictionary, key, targetObject, targetProperty);

			return resource;
		}

		static bool TryGetResource(string key, ReadOnlySpan<object> parentObjects, out object resource, out ResourceDictionary resourceDictionary)
		{
			resource = null;
			resourceDictionary = null;

			foreach (var p in parentObjects)
			{
				var resDict = p is IResourcesProvider irp && irp.IsResourcesCreated ? irp.Resources : p as ResourceDictionary;
				if (resDict == null)
					continue;
				if (resDict.TryGetValueAndSource(key, out resource, out resourceDictionary))
					return true;
			}
			return false;
		}

		static bool TryGetApplicationLevelResource(string key, out object resource, out ResourceDictionary resourceDictionary)
		{
			resource = null;
			resourceDictionary = null;
			return Application.Current != null
				&& ((IResourcesProvider)Application.Current).IsResourcesCreated
				&& Application.Current.Resources.TryGetValueAndSource(key, out resource, out resourceDictionary);
		}
	}
}