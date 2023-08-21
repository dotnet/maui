// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.Maui.Animations;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	class AsyncTicker : Ticker
	{
		bool _enabled;
		bool _systemEnabled = true;

		public override bool SystemEnabled => _systemEnabled;

		public void SetEnabled(bool enabled)
		{
			_systemEnabled = enabled;
			_enabled = enabled;
		}

		public override async void Start()
		{
			_enabled = true;

			while (_enabled)
			{
				Fire?.Invoke();
				await Task.Delay(16);
			}
		}

		public override void Stop()
		{
			_enabled = false;
		}
	}
}