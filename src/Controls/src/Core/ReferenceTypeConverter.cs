using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Xaml.Internals;

namespace Microsoft.Maui.Controls
{
	public sealed class ReferenceTypeConverter : TypeConverter, IExtendedTypeConverter
	{

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

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

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
			=> throw new NotImplementedException();

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
			=> throw new NotSupportedException();
	}
}