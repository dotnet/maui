using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/TypeTypeConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.TypeTypeConverter']/Docs/*" />
	[ProvideCompiled("Microsoft.Maui.Controls.XamlC.TypeTypeConverter")]
	public sealed class TypeTypeConverter : TypeConverter, IExtendedTypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> false;

		object IExtendedTypeConverter.ConvertFromInvariantString(string value, IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			if (!(serviceProvider.GetService(typeof(IXamlTypeResolver)) is IXamlTypeResolver typeResolver))
				throw new ArgumentException("No IXamlTypeResolver in IServiceProvider");

			return typeResolver.Resolve(value, serviceProvider);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
			=> throw new NotImplementedException();

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
			=> throw new NotSupportedException();
	}
}
