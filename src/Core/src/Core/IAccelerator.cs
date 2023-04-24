using System.Collections.Generic;

namespace Microsoft.Maui
{
	public interface IAccelerator
	{
		public IEnumerable<string> Modifiers { get; }

		public IEnumerable<string> Keys { get; }
	}
}
