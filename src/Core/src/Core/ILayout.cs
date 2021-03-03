using System.Collections.Generic;

namespace Microsoft.Maui
{
	public interface ILayout : IView
	{
		IReadOnlyList<IView> Children { get; }
		ILayoutHandler LayoutHandler { get; }

		void Add(IView child);
		void Remove(IView child);
	}
}