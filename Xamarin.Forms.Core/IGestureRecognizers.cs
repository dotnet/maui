using System.Collections.Generic;

namespace Xamarin.Forms
{
	public interface IGestureRecognizers
	{
		IList<IGestureRecognizer> GestureRecognizers { get; }
	}
}