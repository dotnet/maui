using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms.Core.UnitTests
{
	public class MockDispatcher : IDispatcher
	{
		public void BeginInvokeOnMainThread(Action action)
		{
			Device.BeginInvokeOnMainThread(action);
		}

		bool IDispatcher.IsInvokeRequired => Device.IsInvokeRequired;
	}
}