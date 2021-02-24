using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	public interface IGestureRecognizers
	{
		IList<IGestureRecognizer> GestureRecognizers { get; }
	}
}