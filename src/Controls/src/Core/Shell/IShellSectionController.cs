using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
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

		ReadOnlyCollection<ShellContent> GetItems();

		event NotifyCollectionChangedEventHandler ItemsCollectionChanged;
	}
}
