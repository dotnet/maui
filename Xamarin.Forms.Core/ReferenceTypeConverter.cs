using System;
using System.Globalization;

using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Xaml.Internals;

namespace Xamarin.Forms
{
	public sealed class ReferenceTypeConverter : TypeConverter, IExtendedTypeConverter
	{
		object IExtendedTypeConverter.ConvertFrom(CultureInfo culture, object value, IServiceProvider serviceProvider)
		{
			return ((IExtendedTypeConverter)this).ConvertFromInvariantString(value as string, serviceProvider);
		}

		object IExtendedTypeConverter.ConvertFromInvariantString(string value, IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));

			var referenceProvider = serviceProvider.GetService<IReferenceProvider>();
			if (referenceProvider != null) {
				return referenceProvider.FindByName(value)
					   ?? throw new XamlParseException($"Can't resolve name '{value}' on Element", serviceProvider?.GetService<IXmlLineInfoProvider>()?.XmlLineInfo ?? new XmlLineInfo());
			}

#pragma warning disable CS0612 // Type or member is obsolete
			//legacy path
			var namescopeprovider = serviceProvider.GetService(typeof(INameScopeProvider)) as INameScopeProvider;
			var valueProvider = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideParentValues;
			if (valueProvider == null)
				throw new ArgumentException("serviceProvider does not provide an IProvideValueTarget");
			if (namescopeprovider != null && namescopeprovider.NameScope != null) {
				var element = namescopeprovider.NameScope.FindByName(value);
				if (element != null)
					return element;
			}

			foreach (var target in valueProvider.ParentObjects) {
				var ns = target as INameScope;
				if (ns == null)
					continue;
				var element = ns.FindByName(value);
				if (element != null)
					return element;
			}
			throw new Exception("Can't resolve name on Element");
#pragma warning restore CS0612 // Type or member is obsolete
		}

		public override object ConvertFromInvariantString(string value)
		{
			throw new NotImplementedException();
		}
	}
}