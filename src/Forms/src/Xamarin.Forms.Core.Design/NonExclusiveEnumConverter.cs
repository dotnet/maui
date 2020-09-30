using System.ComponentModel;

namespace Xamarin.Forms.Core.Design
{
	internal class NonExclusiveEnumConverter<T> : EnumConverter<T>
	{
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return false;
		}
	}
}
