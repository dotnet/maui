using System.Collections.Generic;

namespace Microsoft.Maui
{
	public interface IApplication
	{
		IReadOnlyList<IWindow> Windows { get; }
		IWindow CreateWindow(IActivationState activationState);
	}
}