using System;
using global::Windows.ApplicationModel.Core;
using global::Windows.UI.Core;
using System.Maui;
using System.Maui.Platform.UWP;

namespace System.Maui.Platform.UWP
{
	internal class Dispatcher : IDispatcher
	{
		readonly CoreDispatcher _coreDispatcher;

		public void BeginInvokeOnMainThread(Action action)
		{
			_coreDispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).WatchForError();
		}

		public Dispatcher()
		{
			_coreDispatcher = CoreApplication.GetCurrentView().Dispatcher;
		}

		public Dispatcher(CoreDispatcher coreDispatcher) 
		{
			_coreDispatcher = coreDispatcher;
		}

		bool IDispatcher.IsInvokeRequired => Device.IsInvokeRequired;
	}
}
