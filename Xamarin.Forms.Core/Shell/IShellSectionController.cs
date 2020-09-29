using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public interface IShellSectionController : IElementController
	{
		event EventHandler<NavigationRequestedEventArgs> NavigationRequested;

		Page PresentedPage { get; }

		void AddContentInsetObserver(IShellContentInsetObserver observer);

		void AddDisplayedPageObserver(object observer, Action<Page> callback);

		bool RemoveContentInsetObserver(IShellContentInsetObserver observer);

		bool RemoveDisplayedPageObserver(object observer);

		void SendInsetChanged(Thickness inset, double tabThickness);

		void SendPopping(Task poppingCompleted);
		void SendPoppingToRoot(Task finishedPopping);

		[Obsolete]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void SendPopped();

		[Obsolete]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void SendPopping(Page page);

		[Obsolete]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void SendPopped(Page page);

		ReadOnlyCollection<ShellContent> GetItems();

		event NotifyCollectionChangedEventHandler ItemsCollectionChanged;
	}
}