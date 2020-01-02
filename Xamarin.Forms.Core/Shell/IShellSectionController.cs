using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

		void SendPopped();
		void SendPopping(Page page);
		void SendPopped(Page page);

		ReadOnlyCollection<ShellContent> GetItems();

		event NotifyCollectionChangedEventHandler ItemsCollectionChanged;
	}
}
