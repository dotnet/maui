using System.Collections.Generic;

namespace System.Maui
{
	public interface ILayoutController
	{
		IReadOnlyList<Element> Children { get; }
	}
}