using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
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
