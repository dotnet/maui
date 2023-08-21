// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Microsoft.Maui.Platform
{
	class ActionDisposable : IDisposable
	{
		volatile Action? _action;
		public ActionDisposable(Action action)
		{
			_action = action;
		}

		public void Dispose()
		{
			Interlocked.Exchange(ref _action, null)?.Invoke();
		}
	}
}
