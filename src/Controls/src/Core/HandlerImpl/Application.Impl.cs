using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	public partial class Application : IApplication
	{
		internal List<IWindow> _internalWindows = new();

		public IReadOnlyList<IWindow> Windows => _internalWindows;

		public virtual IWindow CreateWindow(IActivationState activationState)
		{
			throw new NotImplementedException();
		}

	}
}