using System;

namespace Xamarin.Forms
{
	public interface IDispatcher
	{
		void BeginInvokeOnMainThread(Action action);
		bool IsInvokeRequired { get; }
	}
}