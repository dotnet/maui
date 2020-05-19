using System;

namespace System.Maui
{
	public interface IDispatcher
	{ 
		void BeginInvokeOnMainThread(Action action);
		bool IsInvokeRequired { get; }
	}
}
