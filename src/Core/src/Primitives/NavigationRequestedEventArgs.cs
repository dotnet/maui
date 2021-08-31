using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	public class MauiNavigationRequestedEventArgs : EventArgs
	{
		public IReadOnlyList<IView> NavigationStack { get; }

		public MauiNavigationRequestedEventArgs(IReadOnlyList<IView> newNavigationStack, bool animated)
		{
			NavigationStack = newNavigationStack;
			Animated = animated;
		}

		public bool Animated { get; set; }
	}
}