using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="System.ComponentModel.TypeConverter"/> that converts to <see cref="Microsoft.Maui.Controls.FileImageSource"/>.</summary>
	public sealed class FileImageSourceConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(string);

		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			var strValue = value?.ToString();
			if (strValue != null)
				return (FileImageSource)ImageSource.FromFile(strValue);

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", strValue, typeof(FileImageSource)));
		}

		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (value is not FileImageSource fis)
				throw new NotSupportedException();
			return fis.File;
		}
	}
}
