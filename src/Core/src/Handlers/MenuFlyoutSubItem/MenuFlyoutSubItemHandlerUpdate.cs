// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Communicates information from an IMenuFlyoutSubItem about updates to an IMenuFlyoutSubItemHandler
	/// </summary>
	public record MenuFlyoutSubItemHandlerUpdate(int Index, IMenuElement MenuElement);
}
