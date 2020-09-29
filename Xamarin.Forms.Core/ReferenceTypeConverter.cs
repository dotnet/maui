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
			if (referenceProvider != null)
				return referenceProvider.FindByName(value) ?? throw new XamlParseException($"Can't resolve name '{value}' on Element", serviceProvider);

#pragma warning disable CS0612 // Type or member is obsolete
			//legacy path
			if (!(serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideParentValues valueProvider))
				throw new ArgumentException("serviceProvider does not provide an IProvideValueTarget");
			if (serviceProvider.GetService(typeof(INameScopeProvider)) is INameScopeProvider namescopeprovider && namescopeprovider.NameScope != null)
			{
				var element = namescopeprovider.NameScope.FindByName(value);
				if (element != null)
					return element;
			}

			foreach (var target in valueProvider.ParentObjects)
			{
				if (!(target is INameScope ns))
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