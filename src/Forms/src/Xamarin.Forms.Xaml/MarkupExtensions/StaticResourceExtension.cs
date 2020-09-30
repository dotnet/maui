using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Xaml
{
	[ContentProperty(nameof(Key))]
	public sealed class StaticResourceExtension : IMarkupExtension
	{
		public string Key { get; set; }
		public object ProvideValue(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			if (Key == null)
				throw new XamlParseException("you must specify a key in {StaticResource}", serviceProvider);
			if (!(serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideParentValues valueProvider))
				throw new ArgumentException();

			var xmlLineInfo = serviceProvider.GetService(typeof(IXmlLineInfoProvider)) is IXmlLineInfoProvider xmlLineInfoProvider ? xmlLineInfoProvider.XmlLineInfo : null;

			if (!TryGetResource(Key, valueProvider.ParentObjects, out var resource, out var resourceDictionary)
				&& !TryGetApplicationLevelResource(Key, out resource, out resourceDictionary))
				throw new XamlParseException($"StaticResource not found for key {Key}", xmlLineInfo);

			Diagnostics.ResourceDictionaryDiagnostics.OnStaticResourceResolved(resourceDictionary, Key, valueProvider.TargetObject, valueProvider.TargetProperty);

			return CastTo(resource, valueProvider.TargetProperty);
		}

		//used by X.HR.F
		internal static object CastTo(object value, object targetProperty)
		{
			var bp = targetProperty as BindableProperty;
			var pi = targetProperty as PropertyInfo;
			var propertyType = bp?.ReturnType ?? pi?.PropertyType;
			if (propertyType == null)
			{
				if (value.GetType().GetTypeInfo().IsGenericType && (value.GetType().GetGenericTypeDefinition() == typeof(OnPlatform<>) || value.GetType().GetGenericTypeDefinition() == typeof(OnIdiom<>)))
				{
					// This is only there to support our backward compat story with pre 2.3.3 compiled Xaml project who was not providing TargetProperty
					var method = value.GetType().GetRuntimeMethod("op_Implicit", new[] { value.GetType() });
					value = method.Invoke(null, new[] { value });
				}
				return value;
			}
			if (propertyType.IsAssignableFrom(value.GetType()))
				return value;
			var implicit_op = value.GetType().GetImplicitConversionOperator(fromType: value.GetType(), toType: propertyType)
							?? propertyType.GetImplicitConversionOperator(fromType: value.GetType(), toType: propertyType);
			if (implicit_op != null)
				return implicit_op.Invoke(value, new[] { value });

			//Special case for https://bugzilla.xamarin.com/show_bug.cgi?id=59818
			//On OnPlatform, check for an opImplicit from the targetType
			if (Device.Flags != null
				&& Device.Flags.Contains("xamlDoubleImplicitOpHack")
				&& value.GetType().GetTypeInfo().IsGenericType
				&& (value.GetType().GetGenericTypeDefinition() == typeof(OnPlatform<>)))
			{
				var tType = value.GetType().GenericTypeArguments[0];
				var opImplicit = tType.GetImplicitConversionOperator(fromType: tType, toType: propertyType)
								?? propertyType.GetImplicitConversionOperator(fromType: tType, toType: propertyType);

				if (opImplicit != null)
				{
					//convert the OnPlatform<T> to T
					var opPlatformImplicitConversionOperator = value.GetType().GetImplicitConversionOperator(fromType: value.GetType(), toType: tType);
					value = opPlatformImplicitConversionOperator.Invoke(null, new[] { value });

					//and convert to toType
					value = opImplicit.Invoke(null, new[] { value });
					return value;
				}
			}

			return value;
		}

		bool TryGetResource(string key, IEnumerable<object> parentObjects, out object resource, out ResourceDictionary resourceDictionary)
		{
			resource = null;
			resourceDictionary = null;

			foreach (var p in parentObjects)
			{
				var resDict = p is IResourcesProvider irp && irp.IsResourcesCreated ? irp.Resources : p as ResourceDictionary;
				if (resDict == null)
					continue;
				if (resDict.TryGetValueAndSource(Key, out resource, out resourceDictionary))
					return true;
			}
			return false;
		}

		bool TryGetApplicationLevelResource(string key, out object resource, out ResourceDictionary resourceDictionary)
		{
			resource = null;
			resourceDictionary = null;
			return Application.Current != null && ((IResourcesProvider)Application.Current).IsResourcesCreated && Application.Current.Resources.TryGetValueAndSource(key, out resource, out resourceDictionary);
		}
	}
}