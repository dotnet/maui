using System;

namespace Xamarin.Forms
{
	public interface IOpenGlViewController : IViewController
	{
		event EventHandler DisplayRequested;
	}
}