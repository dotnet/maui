#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class View
	{

#if __ANDROID__
		GestureManager? _gestureManager; 
		private protected override void OnAttachedHandlerCore()
		{
			base.OnAttachedHandlerCore();
			_gestureManager?.Dispose();
			_gestureManager = new GestureManager(Handler);
		}
#endif

		private protected override void OnDetachingHandlerCore()
		{
			base.OnDetachingHandlerCore();

#if __ANDROID__
			_gestureManager?.Dispose();
			_gestureManager = null;
#endif

		}
	}
}
