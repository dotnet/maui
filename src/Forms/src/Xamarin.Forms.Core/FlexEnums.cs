using System;
using System.Globalization;

namespace Xamarin.Forms
{
	[TypeConverter(typeof(FlexJustifyTypeConverter))]
	public enum FlexJustify
	{
		Start = Flex.Justify.Start,
		Center = Flex.Justify.Center,
		End = Flex.Justify.End,
		SpaceBetween = Flex.Justify.SpaceBetween,
		SpaceAround = Flex.Justify.SpaceAround,
		SpaceEvenly = Flex.Justify.SpaceEvenly,
	}

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
	}

	public enum FlexPosition
	{
		Relative = Flex.Position.Relative,
		Absolute = Flex.Position.Absolute,
	}

	[TypeConverter(typeof(FlexDirectionTypeConverter))]
	public enum FlexDirection
	{
		Column = Flex.Direction.Column,
		ColumnReverse = Flex.Direction.ColumnReverse,
		Row = Flex.Direction.Row,
		RowReverse = Flex.Direction.RowReverse,
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
	}

	[TypeConverter(typeof(FlexAlignContentTypeConverter))]
	public enum FlexAlignContent
	{
		Stretch = Flex.AlignContent.Stretch,
		Center = Flex.AlignContent.Center,
		Start = Flex.AlignContent.Start,
		End = Flex.AlignContent.End,
		SpaceBetween = Flex.AlignContent.SpaceBetween,
		SpaceAround = Flex.AlignContent.SpaceAround,
		SpaceEvenly = Flex.AlignContent.SpaceEvenly,
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
	}

	[TypeConverter(typeof(FlexAlignItemsTypeConverter))]
	public enum FlexAlignItems
	{
		Stretch = Flex.AlignItems.Stretch,
		Center = Flex.AlignItems.Center,
		Start = Flex.AlignItems.Start,
		End = Flex.AlignItems.End,
		//Baseline = Flex.AlignItems.Baseline,
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
	}

	[TypeConverter(typeof(FlexAlignSelfTypeConverter))]
	public enum FlexAlignSelf
	{
		Auto = Flex.AlignSelf.Auto,
		Stretch = Flex.AlignSelf.Stretch,
		Center = Flex.AlignSelf.Center,
		Start = Flex.AlignSelf.Start,
		End = Flex.AlignSelf.End,
		//Baseline = Flex.AlignSelf.Baseline,
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
	}

	[TypeConverter(typeof(FlexWrapTypeConverter))]
	public enum FlexWrap
	{
		NoWrap = Flex.Wrap.NoWrap,
		Wrap = Flex.Wrap.Wrap,
		Reverse = Flex.Wrap.WrapReverse,
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
	}

	[TypeConverter(typeof(FlexBasisTypeConverter))]
	public struct FlexBasis
	{
		bool _isLength;
		bool _isRelative;
		public static FlexBasis Auto = new FlexBasis();
		public float Length { get; }
		internal bool IsAuto => !_isLength && !_isRelative;
		internal bool IsRelative => _isRelative;
		public FlexBasis(float length, bool isRelative = false)
		{
			if (length < 0)
				throw new ArgumentException("should be a positive value", nameof(length));
			if (isRelative && length > 1)
				throw new ArgumentException("relative length should be in [0, 1]", nameof(length));
			_isLength = !isRelative;
			_isRelative = isRelative;
			Length = length;
		}

		public static implicit operator FlexBasis(float length)
		{
			return new FlexBasis(length);
		}

		[Xaml.TypeConversion(typeof(FlexBasis))]
		public class FlexBasisTypeConverter : TypeConverter
		{
			public override object ConvertFromInvariantString(string value)
			{
				if (value != null)
				{
					if (value.Equals("auto", StringComparison.OrdinalIgnoreCase))
						return Auto;
					value = value.Trim();
					if (value.EndsWith("%", StringComparison.OrdinalIgnoreCase) && float.TryParse(value.Substring(0, value.Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out float relflex))
						return new FlexBasis(relflex / 100, isRelative: true);
					if (float.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out float flex))
						return new FlexBasis(flex);
				}
				throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(FlexBasis)));
			}
		}
	}
}