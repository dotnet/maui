using System.ComponentModel;

namespace System.Maui.Core.Design
{
	internal class NonExclusiveEnumConverter<T> : EnumConverter<T>
	{
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return false;
		}
	}
}
