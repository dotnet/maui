using System.Collections.Generic;

namespace System.Maui.Internals
{
	public interface IGestureController
	{
		IList<GestureElement> GetChildElements(Point point);
		
		IList<IGestureRecognizer> CompositeGestureRecognizers { get; }		
	}
}