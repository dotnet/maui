using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface IGestureController
	{
		IList<IGestureView> GetChildElements(Point point);

		IList<IGestureRecognizer> CompositeGestureRecognizers { get; }
	}
}