using System.Collections.Generic;

namespace Xamarin.Forms
{
	public interface ILayoutController
	{
		IReadOnlyList<Element> Children { get; }
	}
}