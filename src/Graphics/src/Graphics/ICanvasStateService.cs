// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Graphics
{
	public interface ICanvasStateService<TState>
		where TState : CanvasState
	{
		TState CreateNew(object context);

		TState CreateCopy(TState prototype);
	}
}
