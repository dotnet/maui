using System.ComponentModel;

namespace Microsoft.Maui.Controls.Design
{
	internal class NonExclusiveEnumConverter<T> : EnumConverter<T>
	{
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return false;
		}
	}
}
