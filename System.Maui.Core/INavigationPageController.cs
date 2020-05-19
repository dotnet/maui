using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Maui.Internals;

namespace System.Maui
{
	public interface INavigationPageController
	{
		Task<Page> RemoveAsyncInner(Page page, bool animated, bool fast);

		Page Peek(int depth = 0);

		IEnumerable<Page> Pages { get; }

		int StackDepth { get; }

		Task<Page> PopAsyncInner(bool animated, bool fast = false);

		event EventHandler<NavigationRequestedEventArgs> InsertPageBeforeRequested;

		event EventHandler<NavigationRequestedEventArgs> PopRequested;

		event EventHandler<NavigationRequestedEventArgs> PopToRootRequested;

		event EventHandler<NavigationRequestedEventArgs> PushRequested;

		event EventHandler<NavigationRequestedEventArgs> RemovePageRequested;
	}
}