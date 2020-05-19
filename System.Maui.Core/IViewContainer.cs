using System.Collections.Generic;

namespace System.Maui
{
	public interface IViewContainer<T> where T : VisualElement
	{
		IList<T> Children { get; }
	}
}