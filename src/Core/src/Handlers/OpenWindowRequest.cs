// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Handlers
{
#if WINDOWS
	public record OpenWindowRequest(IPersistedState? State = null, UI.Xaml.LaunchActivatedEventArgs? LaunchArgs = null);
#else
	public record OpenWindowRequest(IPersistedState? State = null);
#endif
}