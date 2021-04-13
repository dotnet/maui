using System;
using System.Collections.Generic;
namespace Microsoft.Maui.Controls
{
	public partial class Application : IApplication
	{
		public List<IWindow> InternalWindows = new List<IWindow>();

		public IReadOnlyList<IWindow> Windows => null;

		public virtual IWindow CreateWindow(IActivationState activationState)
		{
			throw new NotImplementedException();
		}

	}
}