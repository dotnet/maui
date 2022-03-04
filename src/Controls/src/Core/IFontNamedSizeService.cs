using System;

namespace Microsoft.Maui.Controls.Internals
{
	public interface IFontNamedSizeService
	{
		double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes);
	}
}