using System;

namespace Microsoft.Maui.Controls
{
	public interface IDispatcher
	{
		void BeginInvokeOnMainThread(Action action);
		bool IsInvokeRequired { get; }
	}
}
