using System;
using System.Globalization;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms
{
	[Xaml.ProvideCompiled("Xamarin.Forms.Core.XamlC.TypeTypeConverter")]
	public sealed class TypeTypeConverter : TypeConverter, IExtendedTypeConverter
	{
		[Obsolete("Use ConvertFromInvariantString (string, IServiceProvider)")]
		object IExtendedTypeConverter.ConvertFrom(CultureInfo culture, object value, IServiceProvider serviceProvider)
		{
			return ((IExtendedTypeConverter)this).ConvertFromInvariantString((string)value, serviceProvider);
		}

		object IExtendedTypeConverter.ConvertFromInvariantString(string value, IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException("serviceProvider");
			var typeResolver = serviceProvider.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver;
			if (typeResolver == null)
				throw new ArgumentException("No IXamlTypeResolver in IServiceProvider");

			return typeResolver.Resolve(value, serviceProvider);
		}

		public override object ConvertFromInvariantString(string value)
		{
			throw new NotImplementedException();
		}
	}
}