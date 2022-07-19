using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Maui.Controls.Xaml
{
	[ContentProperty(nameof(Key))]
	public sealed class AppThemeExtension : IMarkupExtension<BindingBase>
	{
		public string Key { get; set; }

		public BindingBase ProvideValue(IServiceProvider serviceProvider)
		{
			if (serviceProvider is null)
				throw new ArgumentNullException(nameof(serviceProvider));
			if (Key is null)
				throw new XamlParseException("you must specify a key in {AppThemeColor}", serviceProvider);
			if (serviceProvider.GetService(typeof(IProvideValueTarget)) is not IProvideParentValues valueProvider)
				throw new ArgumentException(null, nameof(serviceProvider));

			if (!TryGetResource(Key, valueProvider.ParentObjects, out var resource, out var resourceDictionary)
				&& !TryGetApplicationLevelResource(Key, out resource, out resourceDictionary))
			{
				var xmlLineInfo = serviceProvider.GetService(typeof(IXmlLineInfoProvider)) is IXmlLineInfoProvider xmlLineInfoProvider ? xmlLineInfoProvider.XmlLineInfo : null;
				throw new XamlParseException($"StaticResource not found for key {Key}", xmlLineInfo);
			}
			else if(resource is AppThemeColor color)
			{
				return color.GetBinding();
			}
			else
			{
				var xmlLineInfo = serviceProvider.GetService(typeof(IXmlLineInfoProvider)) is IXmlLineInfoProvider xmlLineInfoProvider ? xmlLineInfoProvider.XmlLineInfo : null;
				throw new XamlParseException($"StaticResource for key {Key} is not an AppThemeColor", xmlLineInfo);
			}
		}

		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) =>
			ProvideValue(serviceProvider);

		static bool TryGetResource(string key, IEnumerable<object> parentObjects, out object resource, out ResourceDictionary resourceDictionary)
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
			// TODO: Remove reference to Application.Current
			return Application.Current != null
				&& ((IResourcesProvider)Application.Current).IsResourcesCreated
				&& Application.Current.Resources.TryGetValueAndSource(key, out resource, out resourceDictionary);
		}
	}
}
