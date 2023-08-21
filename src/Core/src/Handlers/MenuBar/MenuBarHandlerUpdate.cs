// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Communicates information from an IMenuBar about updates to an IMenuBarHandler
	/// </summary>
	public record MenuBarHandlerUpdate(int Index, IMenuBarItem MenuBarItem);
}
