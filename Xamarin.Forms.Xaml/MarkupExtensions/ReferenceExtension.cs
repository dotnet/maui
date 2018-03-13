using System;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml.Internals;

namespace Xamarin.Forms.Xaml
{
	[ContentProperty("Name")]
	public class ReferenceExtension : IMarkupExtension
	{
		public string Name { get; set; }

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			var valueProvider = serviceProvider.GetService(typeof (IProvideValueTarget)) as IProvideParentValues;
			if (valueProvider == null)
				throw new ArgumentException("serviceProvider does not provide an IProvideValueTarget");
			var namescopeprovider = serviceProvider.GetService(typeof (INameScopeProvider)) as INameScopeProvider;
			if (namescopeprovider != null && namescopeprovider.NameScope != null)
			{
				var value = namescopeprovider.NameScope.FindByName(Name);
				if (value != null)
					return value;
			}

			foreach (var target in valueProvider.ParentObjects)
			{
				var ns = target as INameScope;
				if (ns == null)
					continue;
				var value = ns.FindByName(Name);
				if (value != null)
					return value;
			}

			var lineInfo = (serviceProvider?.GetService(typeof(IXmlLineInfoProvider)) as IXmlLineInfoProvider)?.XmlLineInfo ?? new XmlLineInfo();
			throw new XamlParseException($"Can not find the object referenced by `{Name}`", lineInfo);
		}
	}
}