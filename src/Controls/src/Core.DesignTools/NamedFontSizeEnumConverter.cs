using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Core.Design
{
	internal class NamedFontSizeEnumConverter : EnumConverter
	{
		public NamedFontSizeEnumConverter() : base(Type.GetType("Microsoft.Maui.NamedSize,Microsoft.Maui"))
		{
		}

		// Allow more than just the enum values
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			=> false;
	}
}
