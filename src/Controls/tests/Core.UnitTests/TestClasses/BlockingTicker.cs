// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.Maui.Animations;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	class BlockingTicker : Ticker
	{
		bool _enabled;

		public override void Start()
		{
			_enabled = true;

			while (_enabled)
			{
				Fire?.Invoke();
				Task.Delay(16).Wait();
			}
		}

		public override void Stop()
		{
			_enabled = false;
		}
	}
}