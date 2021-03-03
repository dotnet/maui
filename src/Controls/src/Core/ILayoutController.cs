using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	public interface ILayoutController
	{
		IReadOnlyList<Element> Children { get; }
	}
}