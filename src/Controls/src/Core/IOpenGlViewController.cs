using System;

namespace Microsoft.Maui.Controls
{
	public interface IOpenGlViewController : IViewController
	{
		event EventHandler DisplayRequested;
	}
}