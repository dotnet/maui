// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Graphics.Platform
{
	public class PlatformCanvasStateService : ICanvasStateService<PlatformCanvasState>
	{
		public PlatformCanvasState CreateNew(object context) =>
			new PlatformCanvasState();

		public PlatformCanvasState CreateCopy(PlatformCanvasState prototype) =>
			new PlatformCanvasState(prototype);
	}
}
