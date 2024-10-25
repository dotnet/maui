#if WINDOWS
using Microsoft.UI.Composition;

namespace Microsoft.Maui
{
	internal interface IAlphaMaskProvider
	{
		CompositionBrush? GetAlphaMask();
	}
}
#endif
