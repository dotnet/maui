// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a menu item that displays a sub-menu in a MenuFlyout view.
	/// </summary>
	public interface IMenuFlyoutSubItem : IMenuFlyoutItem, IList<IMenuElement>
	{

	}
}