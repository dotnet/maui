using System;

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
				if (ve == null)
					continue;
				if (ve.Resources == null)
					continue;
				object res;
				if (ve.Resources.TryGetValue(Key, out res))
					return res;
			}
			if (Application.Current != null && Application.Current.Resources != null &&
			    Application.Current.Resources.ContainsKey(Key))
				return Application.Current.Resources[Key];

			throw new XamlParseException(string.Format("StaticResource not found for key {0}", Key), xmlLineInfo);
		}
	}
}