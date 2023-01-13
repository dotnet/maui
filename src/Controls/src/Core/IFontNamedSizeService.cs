#nullable disable
using System;

namespace Microsoft.Maui.Controls.Internals
{
	[Obsolete]
	public interface IFontNamedSizeService
	{
		double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes);
	}
}