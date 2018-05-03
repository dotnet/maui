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
			var referenceProvider = serviceProvider.GetService<IReferenceProvider>();
			if (referenceProvider != null)
				return referenceProvider.FindByName(Name)
					   ?? throw new XamlParseException($"Can not find the object referenced by `{Name}`", serviceProvider?.GetService<IXmlLineInfoProvider>()?.XmlLineInfo ?? new XmlLineInfo());

#pragma warning disable CS0612 // Type or member is obsolete
			//legacy path. could be hit by code processed by previous versions of XamlC
			var valueProvider = serviceProvider.GetService<IProvideValueTarget>() as IProvideParentValues
											   ?? throw new ArgumentException("serviceProvider does not provide an IProvideValueTarget");
			var namescopeprovider = serviceProvider.GetService<INameScopeProvider>();
			if (namescopeprovider != null && namescopeprovider.NameScope != null) {
				var value = namescopeprovider.NameScope.FindByName(Name);
				if (value != null)
					return value;
			}

			foreach (var target in valueProvider.ParentObjects) {
				var bo = target as BindableObject;
				if (bo == null)
					continue;
				var ns = NameScope.GetNameScope(bo) as INameScope;
				if (ns == null)
					continue;
				var value = ns.FindByName(Name);
				if (value != null)
					return value;
			}

			var lineInfo = serviceProvider?.GetService<IXmlLineInfoProvider>()?.XmlLineInfo ?? new XmlLineInfo();
			throw new XamlParseException($"Can not find the object referenced by `{Name}`", lineInfo);
#pragma warning restore CS0612 // Type or member is obsolete
		}
	}
}