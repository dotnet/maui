using System.Collections.Generic;

namespace Xamarin.Forms.Internals
{
	public interface IGestureController
	{
		IList<GestureElement> GetChildElements(Point point);

		IList<IGestureRecognizer> CompositeGestureRecognizers { get; }
	}
}