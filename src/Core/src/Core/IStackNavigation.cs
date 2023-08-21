// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides stack based navigation for the .NET MAUI app.
	/// </summary>
	public interface IStackNavigation
	{
		void RequestNavigation(NavigationRequest eventArgs);
		void NavigationFinished(IReadOnlyList<IView> newStack);
	}
}
