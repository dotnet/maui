using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Design
{
	public class FlexJustifyTypeConverter : KnownValuesDesignTypeConverter
	{
		protected override bool ExclusiveToKnownValues => true;

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

	public class FlexDirectionTypeConverter : KnownValuesDesignTypeConverter
	{
		protected override bool ExclusiveToKnownValues => true;

		protected override string[] KnownValues
			=> new[] {
				"Column",
				"ColumnReverse",
				"Row",
				"RowReverse",
		};
	}

	public class FlexAlignContentTypeConverter : KnownValuesDesignTypeConverter
	{
		protected override bool ExclusiveToKnownValues => true;

		protected override string[] KnownValues
			=> new[] {
				"Stretch",
				"Center",
				"Start",
				"End",
				"SpaceBetween",
				"SpaceAroun",
				"SpaceEvenly",
		};
	}


	public class FlexAlignItemsTypeConverter : KnownValuesDesignTypeConverter
	{
		protected override bool ExclusiveToKnownValues => true;

		protected override string[] KnownValues
			=> new[] {
				"Stretch",
				"Center",
				"Start",
				"End",
		};
	}


	public class FlexAlignSelfTypeConverter : KnownValuesDesignTypeConverter
	{
		protected override bool ExclusiveToKnownValues => true;

		protected override string[] KnownValues
			=> new[] {
				"Auto",
				"Stretch",
				"Center",
				"Start",
				"End",
		};
	}


	public class FlexWrapTypeConverter : KnownValuesDesignTypeConverter
	{
		protected override bool ExclusiveToKnownValues => true;

		protected override string[] KnownValues
			=> new[] {
				"NoWrap",
				"Wrap",
				"Reverse",
		};
	}


	public class FlexBasisTypeConverter : KnownValuesDesignTypeConverter
	{
		protected override string[] KnownValues
			=> new[] {
			    "Auto",
		};

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
