using System.Collections.Generic;

namespace Xamarin.Forms
{
	public interface IViewContainer<T> where T : VisualElement
	{
		IList<T> Children { get; }
	}
}