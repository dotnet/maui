#nullable enable
using System;

namespace Microsoft.Maui.Dispatching
{
	public class Dispatcher : IDispatcher
	{
		public bool IsInvokeRequired =>
			throw new NotImplementedException();

		public void BeginInvokeOnMainThread(Action action)
		{
			throw new NotImplementedException();
		}
	}
}