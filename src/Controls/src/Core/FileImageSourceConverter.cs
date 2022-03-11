using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/FileImageSourceConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.FileImageSourceConverter']/Docs" />
	public sealed class FileImageSourceConverter : TypeConverter
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/FileImageSourceConverter.xml" path="//Member[@MemberName='CanConvertFrom']/Docs" />
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		/// <include file="../../docs/Microsoft.Maui.Controls/FileImageSourceConverter.xml" path="//Member[@MemberName='CanConvertTo']/Docs" />
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> true;

		/// <include file="../../docs/Microsoft.Maui.Controls/FileImageSourceConverter.xml" path="//Member[@MemberName='ConvertFrom']/Docs" />
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();
			if (strValue != null)
				return (FileImageSource)ImageSource.FromFile(strValue);

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", strValue, typeof(FileImageSource)));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/FileImageSourceConverter.xml" path="//Member[@MemberName='ConvertTo']/Docs" />
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not FileImageSource fis)
				throw new NotSupportedException();
			return fis.File;
		}
	}
}