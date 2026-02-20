using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Design
{
	/// <summary>
	/// Provides design-time type conversion for FlexJustify values.
	/// </summary>
	public class FlexJustifyDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		/// <inheritdoc/>
		protected override bool ExclusiveToKnownValues => true;

		/// <inheritdoc/>
		protected override string[] KnownValues
			=> new[] {
				"Start",
				"Center",
				"End",
				"SpaceBetween",
				"SpaceAround",
				"SpaceEvenly",
			};
	}

	/// <summary>
	/// Provides design-time type conversion for FlexDirection values.
	/// </summary>
	public class FlexDirectionDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		/// <inheritdoc/>
		protected override bool ExclusiveToKnownValues => true;

		/// <inheritdoc/>
		protected override string[] KnownValues
			=> new[] {
				"Column",
				"ColumnReverse",
				"Row",
				"RowReverse",
		};
	}

	/// <summary>
	/// Provides design-time type conversion for FlexAlignContent values.
	/// </summary>
	public class FlexAlignContentDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		/// <inheritdoc/>
		protected override bool ExclusiveToKnownValues => true;

		/// <inheritdoc/>
		protected override string[] KnownValues
			=> new[] {
				"Stretch",
				"Center",
				"Start",
				"End",
				"SpaceBetween",
				"SpaceAround",
				"SpaceEvenly",
		};
	}


	/// <summary>
	/// Provides design-time type conversion for FlexAlignItems values.
	/// </summary>
	public class FlexAlignItemsDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		/// <inheritdoc/>
		protected override bool ExclusiveToKnownValues => true;

		/// <inheritdoc/>
		protected override string[] KnownValues
			=> new[] {
				"Stretch",
				"Center",
				"Start",
				"End",
		};
	}


	/// <summary>
	/// Provides design-time type conversion for FlexAlignSelf values.
	/// </summary>
	public class FlexAlignSelfDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		/// <inheritdoc/>
		protected override bool ExclusiveToKnownValues => true;

		/// <inheritdoc/>
		protected override string[] KnownValues
			=> new[] {
				"Auto",
				"Stretch",
				"Center",
				"Start",
				"End",
		};
	}


	/// <summary>
	/// Provides design-time type conversion for FlexWrap values.
	/// </summary>
	public class FlexWrapDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		/// <inheritdoc/>
		protected override bool ExclusiveToKnownValues => true;

		/// <inheritdoc/>
		protected override string[] KnownValues
			=> new[] {
				"NoWrap",
				"Wrap",
				"Reverse",
		};
	}


	/// <summary>
	/// Provides design-time type conversion for FlexBasis values.
	/// </summary>
	public class FlexBasisDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		/// <inheritdoc/>
		protected override string[] KnownValues
			=> new[] {
				"Auto",
		};

		/// <inheritdoc/>
		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			if (KnownValues.Any(v => value?.ToString()?.Equals(v, StringComparison.Ordinal) ?? false))
				return true;

			var strValue = value?.ToString().Trim();

			if (strValue is null)
				return false;

			if (strValue.EndsWith("%", StringComparison.OrdinalIgnoreCase) && float.TryParse(strValue.Substring(0, strValue.Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out float relflex))
				return true;

			if (float.TryParse(strValue, NumberStyles.Number, CultureInfo.InvariantCulture, out float flex))
				return true;

			return false;
		}
	}
}
