using System;

namespace Microsoft.Maui.Controls
{
	public partial class Application : IApplication
	{
		public virtual IWindow CreateWindow(IActivationState activationState)
		{
			throw new NotImplementedException();
		}
	}
}