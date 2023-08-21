// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui
{
	public interface IActivationState
	{
		IMauiContext Context { get; }

		IPersistedState State { get; }
	}
}