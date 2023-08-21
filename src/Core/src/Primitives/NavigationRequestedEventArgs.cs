// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Maui
{
	public class NavigationRequest
	{
		public IReadOnlyList<IView> NavigationStack { get; }

		public NavigationRequest(IReadOnlyList<IView> newNavigationStack, bool animated)
		{
			NavigationStack = newNavigationStack;
			Animated = animated;
		}

		public bool Animated { get; }
	}
}