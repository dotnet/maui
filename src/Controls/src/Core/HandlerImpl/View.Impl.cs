#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class View
	{
		GestureManager? _gestureManager;
		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();
			_gestureManager?.Dispose();

			if (Handler != null)
				_gestureManager = new GestureManager(Handler);
		}

		private protected override void OnHandlerChangingCore(HandlerChangingEventArgs args)
		{
			_gestureManager?.Dispose();
			_gestureManager = null;

			base.OnHandlerChangingCore(args);
		}
	}
}
