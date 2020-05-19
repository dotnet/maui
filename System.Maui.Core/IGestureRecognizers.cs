using System.Collections.Generic;

namespace System.Maui
{
	public interface IGestureRecognizers
	{
		IList<IGestureRecognizer> GestureRecognizers { get; }
	}
}