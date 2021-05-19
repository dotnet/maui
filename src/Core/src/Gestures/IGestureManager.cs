using System;

namespace Microsoft.Maui
{
	public interface IGestureManager : IDisposable
	{
		void SetViewHandler(IViewHandler handler);
	}
}