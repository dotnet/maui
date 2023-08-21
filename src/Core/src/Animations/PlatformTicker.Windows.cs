// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Animations
{
	/// <inheritdoc/>
	public class PlatformTicker : Ticker
	{
		/// <inheritdoc/>
		public override void Start()
		{
			CompositionTarget.Rendering += RenderingFrameEventHandler;
		}

		/// <inheritdoc/>
		public override void Stop()
		{
			CompositionTarget.Rendering -= RenderingFrameEventHandler;
		}

		void RenderingFrameEventHandler(object? sender, object? args)
		{
			Fire?.Invoke();
		}
	}
}