using System;
using System.ComponentModel;

namespace Microsoft.Maui.Layouts
{
	[TypeConverter(typeof(Converters.FlexJustifyTypeConverter))]
	public enum FlexJustify
	{
		Start = Flex.Justify.Start,
		Center = Flex.Justify.Center,
		End = Flex.Justify.End,
		SpaceBetween = Flex.Justify.SpaceBetween,
		SpaceAround = Flex.Justify.SpaceAround,
		SpaceEvenly = Flex.Justify.SpaceEvenly,
	}

	public enum FlexPosition
	{
		Relative = Flex.Position.Relative,
		Absolute = Flex.Position.Absolute,
	}

	[TypeConverter(typeof(Converters.FlexDirectionTypeConverter))]
	public enum FlexDirection
	{
		Column = Flex.Direction.Column,
		ColumnReverse = Flex.Direction.ColumnReverse,
		Row = Flex.Direction.Row,
		RowReverse = Flex.Direction.RowReverse,
	}

	[TypeConverter(typeof(Converters.FlexAlignContentTypeConverter))]
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

	[TypeConverter(typeof(Converters.FlexAlignItemsTypeConverter))]
	public enum FlexAlignItems
	{
		Stretch = Flex.AlignItems.Stretch,
		Center = Flex.AlignItems.Center,
		Start = Flex.AlignItems.Start,
		End = Flex.AlignItems.End,
		//Baseline = Flex.AlignItems.Baseline,
	}

	[TypeConverter(typeof(Converters.FlexAlignSelfTypeConverter))]
	public enum FlexAlignSelf
	{
		Auto = Flex.AlignSelf.Auto,
		Stretch = Flex.AlignSelf.Stretch,
		Center = Flex.AlignSelf.Center,
		Start = Flex.AlignSelf.Start,
		End = Flex.AlignSelf.End,
		//Baseline = Flex.AlignSelf.Baseline,
	}

	[TypeConverter(typeof(Converters.FlexWrapTypeConverter))]
	public enum FlexWrap
	{
		NoWrap = Flex.Wrap.NoWrap,
		Wrap = Flex.Wrap.Wrap,
		Reverse = Flex.Wrap.WrapReverse,
	}

	[TypeConverter(typeof(Converters.FlexBasisTypeConverter))]
	public struct FlexBasis : IEquatable<FlexBasis>
	{
		bool _isLength;
		bool _isRelative;
		public float Length { get; }
		internal bool IsAuto => !_isLength && !_isRelative;
		internal bool IsRelative => _isRelative;

		public static readonly FlexBasis Auto;

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
			=> new FlexBasis(length);

		public bool Equals(FlexBasis other) => _isLength == other._isLength && _isRelative == other._isRelative && Length == other.Length;

		public override bool Equals(object? obj) => obj is FlexBasis other && Equals(other);

		public override int GetHashCode() => _isRelative.GetHashCode() ^ Length.GetHashCode();

		public static bool operator ==(FlexBasis left, FlexBasis right) => left.Equals(right);

		public static bool operator !=(FlexBasis left, FlexBasis right) => !(left == right);
	}
}