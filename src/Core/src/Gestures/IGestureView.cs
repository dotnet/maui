using System.Collections.Generic;

namespace Microsoft.Maui
{
	public interface IGestureView
	{
		public IList<IGestureRecognizer> GestureRecognizers { get; }
	}
}