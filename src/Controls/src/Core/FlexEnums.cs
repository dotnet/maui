using System;
using System.Globalization;
using Microsoft.Maui.Layouts;
using Flex = Microsoft.Maui.Layouts.Flex;

namespace Microsoft.Maui.Controls
{
	[Xaml.TypeConversion(typeof(FlexJustify))]
	public class FlexJustifyTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				if (Enum.TryParse(value, true, out FlexJustify justify))
					return justify;
				if (value.Equals("flex-start", StringComparison.OrdinalIgnoreCase))
					return FlexJustify.Start;
				if (value.Equals("flex-end", StringComparison.OrdinalIgnoreCase))
					return FlexJustify.End;
				if (value.Equals("space-between", StringComparison.OrdinalIgnoreCase))
					return FlexJustify.SpaceBetween;
				if (value.Equals("space-around", StringComparison.OrdinalIgnoreCase))
					return FlexJustify.SpaceAround;
			}
			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(FlexJustify)));
		}

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is FlexJustify fj))
				throw new NotSupportedException();
			return fj.ToString();
		}
	}

	[Xaml.TypeConversion(typeof(FlexDirection))]
	public class FlexDirectionTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				if (Enum.TryParse(value, true, out FlexDirection aligncontent))
					return aligncontent;
				if (value.Equals("row-reverse", StringComparison.OrdinalIgnoreCase))
					return FlexDirection.RowReverse;
				if (value.Equals("column-reverse", StringComparison.OrdinalIgnoreCase))
					return FlexDirection.ColumnReverse;
			}
			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(FlexDirection)));
		}

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is FlexDirection fd))
				throw new NotSupportedException();
			return fd.ToString();
		}
	}

	[Xaml.TypeConversion(typeof(FlexAlignContent))]
	public class FlexAlignContentTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				if (Enum.TryParse(value, true, out FlexAlignContent aligncontent))
					return aligncontent;
				if (value.Equals("flex-start", StringComparison.OrdinalIgnoreCase))
					return FlexAlignContent.Start;
				if (value.Equals("flex-end", StringComparison.OrdinalIgnoreCase))
					return FlexAlignContent.End;
				if (value.Equals("space-between", StringComparison.OrdinalIgnoreCase))
					return FlexAlignContent.SpaceBetween;
				if (value.Equals("space-around", StringComparison.OrdinalIgnoreCase))
					return FlexAlignContent.SpaceAround;
			}
			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(FlexAlignContent)));
		}

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is FlexAlignContent fac))
				throw new NotSupportedException();
			return fac.ToString();
		}
	}

	[Xaml.TypeConversion(typeof(FlexAlignItems))]
	public class FlexAlignItemsTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				if (Enum.TryParse(value, true, out FlexAlignItems alignitems))
					return alignitems;
				if (value.Equals("flex-start", StringComparison.OrdinalIgnoreCase))
					return FlexAlignItems.Start;
				if (value.Equals("flex-end", StringComparison.OrdinalIgnoreCase))
					return FlexAlignItems.End;
			}
			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(FlexAlignItems)));
		}

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is FlexAlignItems fai))
				throw new NotSupportedException();
			return fai.ToString();
		}
	}

	[Xaml.TypeConversion(typeof(FlexAlignSelf))]
	public class FlexAlignSelfTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				if (Enum.TryParse(value, true, out FlexAlignSelf alignself))
					return alignself;
				if (value.Equals("flex-start", StringComparison.OrdinalIgnoreCase))
					return FlexAlignSelf.Start;
				if (value.Equals("flex-end", StringComparison.OrdinalIgnoreCase))
					return FlexAlignSelf.End;
			}
			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(FlexAlignSelf)));
		}

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is FlexAlignSelf fes))
				throw new NotSupportedException();
			return fes.ToString();
		}
	}

	[Xaml.TypeConversion(typeof(FlexWrap))]
	public class FlexWrapTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				if (Enum.TryParse(value, true, out FlexWrap wrap))
					return wrap;
				if (value.Equals("wrap-reverse", StringComparison.OrdinalIgnoreCase))
					return FlexWrap.Reverse;
			}
			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(FlexWrap)));
		}

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is FlexWrap fw))
				throw new NotSupportedException();
			return fw.ToString();
		}
	}

	[Xaml.TypeConversion(typeof(FlexBasis))]
	public class FlexBasisTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				if (value.Equals("auto", StringComparison.OrdinalIgnoreCase))
					return FlexBasis.Auto;
				value = value.Trim();
				if (value.EndsWith("%", StringComparison.OrdinalIgnoreCase) && float.TryParse(value.Substring(0, value.Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out float relflex))
					return new FlexBasis(relflex / 100, isRelative: true);
				if (float.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out float flex))
					return new FlexBasis(flex);
			}
			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(FlexBasis)));
		}

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is FlexBasis basis))
				throw new NotSupportedException();
			if (basis.IsAuto)
				return "auto";
			if (basis.IsRelative)
				return $"{(basis.Length * 100).ToString(CultureInfo.InvariantCulture)}%";
			return $"{basis.Length.ToString(CultureInfo.InvariantCulture)}";
		}
	}
}