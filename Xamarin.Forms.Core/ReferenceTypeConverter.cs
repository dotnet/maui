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
			var valueProvider = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideParentValues;
			if (valueProvider == null)
				throw new ArgumentException("serviceProvider does not provide an IProvideValueTarget");
			var namescopeprovider = serviceProvider.GetService(typeof(INameScopeProvider)) as INameScopeProvider;
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
		}

		public override object ConvertFromInvariantString(string value)
		{
			throw new NotImplementedException();
		}
	}
}