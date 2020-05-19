using System;

namespace System.Maui
{
	public interface IOpenGlViewController : IViewController
	{
		event EventHandler DisplayRequested;
	}
}