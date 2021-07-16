using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public abstract class StringTypeConverterBase : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> true;
	}
}
