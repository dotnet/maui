// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Communicates information from an IMenuBarItem about updates to an IMenuBarItemHandler
	/// </summary>
	public record MenuBarItemHandlerUpdate(int Index, IMenuElement MenuElement);
}
