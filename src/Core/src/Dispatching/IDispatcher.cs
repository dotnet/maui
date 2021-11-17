using System;

namespace Microsoft.Maui.Dispatching
{
	public interface IDispatcher
	{
		void BeginInvokeOnMainThread(Action action);

		bool IsInvokeRequired { get; }
	}
}