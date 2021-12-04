using System;

namespace Microsoft.Maui.Dispatching
{
	public interface IDispatcher
	{
		bool Dispatch(Action action);

		bool IsDispatchRequired { get; }
	}
}