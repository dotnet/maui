using System.ComponentModel;

namespace Microsoft.Maui.Controls.Core.Design
{
	internal class NonExclusiveEnumConverter<T> : EnumConverter<T>
	{
		// Allow more than just the enum values
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			=> false;
	}
}
