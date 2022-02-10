using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometryConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.PathGeometryConverter']/Docs" />
	public class PathGeometryConverter : TypeConverter
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometryConverter.xml" path="//Member[@MemberName='CanConvertFrom']/Docs" />
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometryConverter.xml" path="//Member[@MemberName='CanConvertTo']/Docs" />
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> false;

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometryConverter.xml" path="//Member[@MemberName='ConvertFrom']/Docs" />
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();
			PathGeometry pathGeometry = new PathGeometry();

			PathFigureCollectionConverter.ParseStringToPathFigureCollection(pathGeometry.Figures, strValue);

			return pathGeometry;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathGeometryConverter.xml" path="//Member[@MemberName='ConvertTo']/Docs" />
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
			=> throw new NotSupportedException();
	}
}