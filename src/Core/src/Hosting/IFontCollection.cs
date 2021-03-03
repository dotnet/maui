using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.Hosting
{
	public interface IFontCollection
		: IList<FontDescriptor>, ICollection<FontDescriptor>, IEnumerable<FontDescriptor>, IEnumerable
	{
	}
}
