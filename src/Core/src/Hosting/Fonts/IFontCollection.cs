using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.Hosting
{
	/// <summary>
	/// A collection of fonts.
	/// </summary>
	public interface IFontCollection : IList<FontDescriptor>, ICollection<FontDescriptor>, IEnumerable<FontDescriptor>, IEnumerable
	{
	}
}
