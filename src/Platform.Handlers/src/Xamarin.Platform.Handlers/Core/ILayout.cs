using System.Collections.Generic;

namespace Xamarin.Platform
{
	public interface ILayout : IView
	{
		IReadOnlyList<IView> Children { get; }
		ILayoutHandler LayoutHandler { get; }

		void Add(IView child);
		void Remove(IView child);
	}
}