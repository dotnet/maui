#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	// This contains the messaging required to communicate with legacy renderers
	/// <summary>A <see cref="Microsoft.Maui.Controls.Page"/> that manages the navigation and user-experience of a stack of other pages.</summary>
	public partial class NavigationPage : INavigationPageController
	{
		async Task<Page> PopAsyncInner(
			bool animated,
			bool fast)
		{
			if (NavigationPageController.StackDepth == 1)
			{
				return null;
			}

			var page = (Page)InternalChildren.Last();
			var previousPage = CurrentPage;
			SendNavigating(NavigationType.Pop, previousPage);
			var removedPage = await RemoveAsyncInner(page, animated, fast);
			SendNavigated(previousPage, NavigationType.Pop);
			return removedPage;
		}

		async Task<Page> RemoveAsyncInner(
			Page page,
			bool animated,
			bool fast)
		{
			if (NavigationPageController.StackDepth == 1)
			{
				return null;
			}

			FireDisappearing(page);

			var args = new NavigationRequestedEventArgs(page, animated);

			var removed = true;

			EventHandler<NavigationRequestedEventArgs> requestPop = _popRequested;
			if (requestPop != null)
			{
				requestPop(this, args);

				if (args.Task != null && !fast)
					removed = await args.Task;
			}

			if (!removed && !fast)
				return CurrentPage;

			bool isLastPage = InternalChildren.Last() == page;
			RemoveFromInnerChildren(page);

			if (isLastPage && InternalChildren.Count > 0)
			{
				FireAppearing((Page)InternalChildren.Last());
			}

			CurrentPage = (Page)InternalChildren.Last();

			if (Popped != null)
				Popped(this, args);

			return page;
		}

		Task<Page> INavigationPageController.PopAsyncInner(bool animated, bool fast)
		{
			return PopAsyncInner(animated, fast);
		}

		Task<Page> INavigationPageController.RemoveAsyncInner(Page page, bool animated, bool fast)
		{
			return RemoveAsyncInner(page, animated, fast);
		}

		EventHandler<NavigationRequestedEventArgs> _popRequested;
		event EventHandler<NavigationRequestedEventArgs> INavigationPageController.PopRequested
		{
			add => _popRequested += value;
			remove => _popRequested -= value;
		}

		EventHandler<NavigationRequestedEventArgs> _popToRootRequested;
		event EventHandler<NavigationRequestedEventArgs> INavigationPageController.PopToRootRequested
		{
			add => _popToRootRequested += value;
			remove => _popToRootRequested -= value;
		}

		EventHandler<NavigationRequestedEventArgs> _pushRequested;
		event EventHandler<NavigationRequestedEventArgs> INavigationPageController.PushRequested
		{
			add => _pushRequested += value;
			remove => _pushRequested -= value;
		}

		EventHandler<NavigationRequestedEventArgs> _removePageRequested;
		event EventHandler<NavigationRequestedEventArgs> INavigationPageController.RemovePageRequested
		{
			add => _removePageRequested += value;
			remove => _removePageRequested -= value;
		}

		EventHandler<NavigationRequestedEventArgs> _insertPageBeforeRequested;
		event EventHandler<NavigationRequestedEventArgs> INavigationPageController.InsertPageBeforeRequested
		{
			add => _insertPageBeforeRequested += value;
			remove => _insertPageBeforeRequested -= value;
		}

		void InsertPageBefore(Page page, Page before)
		{
			if (page == null)
				throw new ArgumentNullException($"{nameof(page)} cannot be null.");

			if (before == null)
				throw new ArgumentNullException($"{nameof(before)} cannot be null.");

			if (!InternalChildren.Contains(before))
				throw new ArgumentException($"{nameof(before)} must be a child of the NavigationPage", nameof(before));

			if (InternalChildren.Contains(page))
				throw new ArgumentException("Cannot insert page which is already in the navigation stack");

			_insertPageBeforeRequested?.Invoke(this, new NavigationRequestedEventArgs(page, before, false));

			int index = InternalChildren.IndexOf(before);
			InternalChildren.Insert(index, page);

			if (index == 0)
				RootPage = page;

			// Shouldn't be required?
			if (Width > 0 && Height > 0)
				ForceLayout();
		}

		async Task PopToRootAsyncInner(bool animated)
		{
			if (NavigationPageController.StackDepth == 1)
				return;

			var previousPage = CurrentPage;
			SendNavigating(NavigationType.PopToRoot, previousPage);
			FireDisappearing(CurrentPage);
			FireAppearing((Page)InternalChildren[0]);

			Element[] childrenToRemove = InternalChildren.Skip(1).ToArray();
			foreach (Element child in childrenToRemove)
			{
				RemoveFromInnerChildren(child);
			}

			CurrentPage = RootPage;

			var args = new NavigationRequestedEventArgs(RootPage, animated);

			EventHandler<NavigationRequestedEventArgs> requestPopToRoot = _popToRootRequested;
			if (requestPopToRoot != null)
			{
				requestPopToRoot(this, args);

				if (args.Task != null)
					await args.Task;
			}

			PoppedToRoot?.Invoke(this, new PoppedToRootEventArgs(RootPage, childrenToRemove.OfType<Page>().ToList()));
			SendNavigated(previousPage, NavigationType.PopToRoot);
		}

		async Task PushAsyncInner(Page page, bool animated)
		{
			if (InternalChildren.Contains(page))
				return;

			var previousPage = CurrentPage;
			var navigationType = DetermineNavigationType();

			SendNavigating(navigationType, previousPage);
			FireDisappearing(CurrentPage);
			FireAppearing(page);

			PushPage(page);

			var args = new NavigationRequestedEventArgs(page, animated);

			EventHandler<NavigationRequestedEventArgs> requestPush = _pushRequested;
			if (requestPush != null)
			{
				requestPush(this, args);

				if (args.Task != null)
					await args.Task;
			}

			SendNavigated(previousPage, navigationType);
			Pushed?.Invoke(this, args);
		}

#if IOS
		// Because iOS currently doesn't use our `IStackNavigationView` structures
		// there are scenarios where the legacy handler needs to alert the xplat
		// code of when a navigation has occurred.
		// For example, initial load and when the user taps the back button
		internal void SendNavigatedFromHandler(Page previousPage, NavigationType navigationType)
		{
			if (CurrentPage.HasNavigatedTo)
				return;

			SendNavigated(previousPage, navigationType);
		}
#endif

		void PushPage(Page page)
		{
			InternalChildren.Add(page);

			if (InternalChildren.Count == 1)
				RootPage = page;

			CurrentPage = page;
		}

		void RemovePage(Page page)
		{
			if (page == null)
				throw new ArgumentNullException($"{nameof(page)} cannot be null.");

			if (page == CurrentPage && CurrentPage == RootPage)
				throw new InvalidOperationException("Cannot remove root page when it is also the currently displayed page.");

			if (page == CurrentPage)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<NavigationPage>()?.LogWarning("RemovePage called for CurrentPage object. This can result in undesired behavior, consider calling PopAsync instead.");
				PopAsync();
				return;
			}

			if (!InternalChildren.Contains(page))
				throw new ArgumentException("Page to remove must be contained on this Navigation Page");

			_removePageRequested?.Invoke(this, new NavigationRequestedEventArgs(page, true));
			RemoveFromInnerChildren(page);

			if (RootPage == page)
				RootPage = (Page)InternalChildren.First();
		}

		class NavigationImpl : NavigationProxy
		{
			readonly Lazy<ReadOnlyCastingList<Page, Element>> _castingList;

			public NavigationImpl(NavigationPage owner)
			{
				Owner = owner;
				_castingList = new Lazy<ReadOnlyCastingList<Page, Element>>(() => new ReadOnlyCastingList<Page, Element>(Owner.InternalChildren));
			}

			NavigationPage Owner { get; }

			protected override IReadOnlyList<Page> GetNavigationStack()
			{
				return _castingList.Value;
			}

			protected override void OnInsertPageBefore(Page page, Page before)
			{
				Owner.InsertPageBefore(page, before);
			}

			protected override Task<Page> OnPopAsync(bool animated)
			{
				return Owner.PopAsync(animated);
			}

			protected override Task OnPopToRootAsync(bool animated)
			{
				return Owner.PopToRootAsync(animated);
			}

			protected override Task OnPushAsync(Page root, bool animated)
			{
				return Owner.PushAsync(root, animated);
			}

			protected override void OnRemovePage(Page page)
			{
				Owner.RemovePage(page);
			}
		}
	}
}
