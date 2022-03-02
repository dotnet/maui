using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Platform
{
	class ActionDisposable : IDisposable
	{
		Action? _action;
		public ActionDisposable(Action action)
		{
			_action = action;
		}

		public void Dispose()
		{
			var action = _action;
			_action = null;
			action?.Invoke();
		}
	}
}
