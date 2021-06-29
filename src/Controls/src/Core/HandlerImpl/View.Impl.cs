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
		private protected override void OnAttachedHandlerCore()
		{
			base.OnAttachedHandlerCore();
			_gestureManager?.Dispose();
			_gestureManager = new GestureManager(Handler);
		}

		private protected override void OnDetachingHandlerCore()
		{
			base.OnDetachingHandlerCore();
			_gestureManager?.Dispose();
			_gestureManager = null;
		}
	}
}
