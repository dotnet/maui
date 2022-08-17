using System.ComponentModel;
using System.Linq;

namespace Microsoft.Maui.Controls.Design
{
	public class GridLengthCollectionDesignTypeConverter : TypeConverter
	{
		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			if (value?.ToString() is string strValue)
			{
				string[] lengths = strValue.Split(',');
				return lengths.All(GridLengthDesignTypeConverter.IsValid);
			}

			return false;
		}
	}
}
