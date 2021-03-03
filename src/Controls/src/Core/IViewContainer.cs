using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	public interface IViewContainer<T> where T : VisualElement
	{
		IList<T> Children { get; }
	}
}