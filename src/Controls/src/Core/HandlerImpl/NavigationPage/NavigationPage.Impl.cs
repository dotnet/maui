using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/NavigationPage.xml" path="Type[@FullName='Microsoft.Maui.Controls.NavigationPage']/Docs" />
	public partial class NavigationPage : INavigationView, IToolbarElement
	{
		// If the user is making overlapping navigation requests this is used to fire once all navigation 
		// events have been processed
		TaskCompletionSource<object> _allPendingNavigationCompletionSource;

		// This is used to process the currently active navigation request
		TaskCompletionSource<object> _currentNavigationCompletionSource;

		int _waitingCount = 0;
		readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);

		partial void Init()
		{
			this.Appearing += OnAppearing;
			this.NavigatingFrom += OnNavigatingFrom;
		}

		Thickness IView.Margin => Thickness.Zero;

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			if (Content is IView view)
			{
				_ = view.Measure(widthConstraint, heightConstraint);
			}

			return new Size(widthConstraint, heightConstraint);
		}

		protected override Size ArrangeOverride(Rectangle bounds)
		{
			Frame = this.ComputeFrame(bounds);

			if (Content is IView view)
			{
				_ = view.Arrange(Frame);
			}

			return Frame.Size;
		}

		void INavigationView.RequestNavigation(NavigationRequest eventArgs)
		{
			Handler?.Invoke(nameof(INavigationView.RequestNavigation), eventArgs);
		}

		// If a native navigation occurs then this syncs up the NavigationStack
		// with the new native Navigation Stack
		void INavigationView.NavigationFinished(IReadOnlyList<IView> newStack)
		{
			// If the user is performing multiple overlapping navigations then we don't want to sync the native stack to our xplat stack
			// We wait until we get to the end of the queue and then we sync up.
			// Otherwise intermediate results will wipe out the navigationstack
			if (_waitingCount <= 1)
			{
				SyncToNavigationStack(newStack);
				CurrentPage = (Page)newStack[newStack.Count - 1];
				RootPage = (Page)newStack[0];
			}

			var completionSource = _currentNavigationCompletionSource;
			CurrentNavigationTask = null;
			_currentNavigationCompletionSource = null;
			completionSource?.SetResult(null);
		}

		void SyncToNavigationStack(IReadOnlyList<IView> newStack)
		{
			for (int i = 0; i < newStack.Count; i++)
			{
				var element = (Element)newStack[i];

				if (InternalChildren.Count < i)
					InternalChildren.Add(element);
				else if (InternalChildren[i] != element)
				{
					int index = InternalChildren.IndexOf(element);
					if (index >= 0)
					{
						InternalChildren.Move(index, i);
					}
					else
					{
						InternalChildren.Insert(i, element);
					}
				}
			}

			while (InternalChildren.Count > newStack.Count)
			{
				InternalChildren.RemoveAt(InternalChildren.Count - 1);
			}
		}

		IView Content => this.CurrentPage;

		IReadOnlyList<IView> NavigationStack => this.Navigation.NavigationStack;


		static void OnCurrentPageChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (oldValue is Page oldPage)
				oldPage.SendDisappearing();

			if (newValue is Page newPage && ((NavigationPage)bindable).HasAppeared)
				newPage.SendAppearing();
		}


		IToolbar INavigationView.Toolbar
		{
			get
			{
				if (this.Toolbar != null)
					return Toolbar;

				var rootPage = this.FindParentWith(x => (x is IWindow te || Navigation.ModalStack.Contains(x)), true);
				if (this.FindParentWith(x => (x is IToolbarElement te && te.Toolbar != null), false) is IToolbarElement te)
				{
					// This means I'm inside a Modal Page so we shouldn't return the Toolbar from the window
					if (rootPage is not IWindow && te is IWindow)
						return null;

					return te.Toolbar;
				}

				return null;
			}
		}

		void OnNavigatingFrom(object sender, EventArgs e)
		{
			// Update the Container level Toolbar with my Toolbar information
			if (this is INavigationView te && te.Toolbar is Toolbar ct)
			{
				// If the root page is being covered by a Modal Page then we don't worry about hiding the nav bar
				bool coveredByModal = te.Toolbar.Parent is Window && Navigation.ModalStack.Count > 0;
				ct.ApplyNavigationPage(this, coveredByModal);
			}
		}

		void OnAppearing(object sender, EventArgs e)
		{
			// Update the Container level Toolbar with my Toolbar information
			if (this is INavigationView te && te.Toolbar is Toolbar ct)
			{
				ct.ApplyNavigationPage(this, HasAppeared);
			}
			// This means the toolbar hasn't been initialized yet
			// This code figures out what level the toolbar gets set on
			else
			{

				// If the root is a flyoutpage then we set the toolbar on the flyout page
				var flyoutPage = this.FindParentOfType<FlyoutPage>();
				if (flyoutPage != null && flyoutPage.Parent is IWindow)
				{
					var toolbar = new Toolbar(flyoutPage);
					toolbar.ApplyNavigationPage(this, true);
					flyoutPage.Toolbar = toolbar;
				}
				// Is the root a modal page?
				else
				{
					// Is the root the window or is this part of a modal stack
					var rootPage = this.FindParentWith(x => (x is IWindow te || Navigation.ModalStack.Contains(x)), true);

					if (rootPage is Window w)
					{
						var toolbar = new Toolbar(w);
						toolbar.ApplyNavigationPage(this, true);
						w.Toolbar = toolbar;
					}
					else if (rootPage is Page p)
					{
						var toolbar = new Toolbar(p);
						toolbar.ApplyNavigationPage(this, true);
						p.Toolbar = toolbar;
					}
				}
			}
		}

		// This is used for navigation events that don't effect the currently visible page
		// InsertPageBefore/RemovePage
		async void SendHandlerUpdate(bool animated)
		{
			try
			{
				Interlocked.Increment(ref _waitingCount);
				await SemaphoreSlim.WaitAsync();
				var trulyReadOnlyNavigationStack = new List<IView>(NavigationStack);
				var request = new NavigationRequest(trulyReadOnlyNavigationStack, animated);
				((INavigationView)this).RequestNavigation(request);
			}
			finally
			{
				Interlocked.Decrement(ref _waitingCount);
				SemaphoreSlim.Release();
			}
		}

		async Task SendHandlerUpdateAsync(
			bool animated,
			Action processStackChanges,
			Action firePostNavigatingEvents,
			Action fireNavigatedEvents)
		{
			if (!_setForMaui || this.IsShimmed())
			{
				return;
			}

			processStackChanges?.Invoke();

			if (Handler == null)
			{
				return;
			}

			try
			{
				Interlocked.Increment(ref _waitingCount);

				// Wait for pending navigation tasks to finish
				await SemaphoreSlim.WaitAsync();

				var currentNavRequestTaskSource = new TaskCompletionSource<object>();
				_allPendingNavigationCompletionSource ??= new TaskCompletionSource<object>();

				if (CurrentNavigationTask == null)
				{
					CurrentNavigationTask = _allPendingNavigationCompletionSource.Task;
				}
				else if (CurrentNavigationTask != _allPendingNavigationCompletionSource.Task)
				{
					throw new InvalidOperationException("Pending Navigations still processing");
				}

				_currentNavigationCompletionSource = currentNavRequestTaskSource;

				// We create a new list to send to the handler because the structure backing 
				// The Navigation stack isn't immutable
				var immutableNavigationStack = new List<IView>(NavigationStack);
				firePostNavigatingEvents?.Invoke();

				// Create the request for the handler
				var request = new NavigationRequest(immutableNavigationStack, animated);
				((INavigationView)this).RequestNavigation(request);

				// Wait for the handler to finish processing the navigation
				// This task completes once the handler calls INavigationView.Finished
				await currentNavRequestTaskSource.Task;
			}
			finally
			{
				if (Interlocked.Decrement(ref _waitingCount) == 0)
				{
					_allPendingNavigationCompletionSource.SetResult(new object());
					_allPendingNavigationCompletionSource = null;
				}

				SemaphoreSlim.Release();
			}

			// Send navigated event to currently visible pages and associated navigation event
			fireNavigatedEvents?.Invoke();
		}

		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();

			if (Handler == null && (this as INavigationView).Toolbar is IToolbar tb)
			{
				tb.Handler = null;
				if (tb.Parent is Window w)
					w.Toolbar = null;
				else if (tb.Parent is Page p)
					p.Toolbar = null;
			}

			if (InternalChildren.Count > 0)
			{
				var navStack = Navigation.NavigationStack;
				var visiblePage = Navigation.NavigationStack[NavigationStack.Count - 1];
				RootPage = navStack[0];
				CurrentPage = visiblePage;

				SendHandlerUpdateAsync(false, null,
				() =>
				{
					FireAppearing(CurrentPage);
				},
				() =>
				{
					SendNavigated(null);
				})
				.FireAndForget(Handler);
			}
		}

		// Once we get all platforms over to the new APIs
		// we can just delete all the code inside NavigationPage.cs that fires "requested" events
		class MauiNavigationImpl : NavigationProxy
		{
			readonly Lazy<ReadOnlyCastingList<Page, Element>> _castingList;

			public MauiNavigationImpl(NavigationPage owner)
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
				if (page == null)
					throw new ArgumentNullException($"{nameof(page)} cannot be null.");

				if (before == null)
					throw new ArgumentNullException($"{nameof(before)} cannot be null.");

				if (!Owner.InternalChildren.Contains(before))
					throw new ArgumentException($"{nameof(before)} must be a child of the NavigationPage", nameof(before));

				if (Owner.InternalChildren.Contains(page))
					throw new ArgumentException("Cannot insert page which is already in the navigation stack");

				int index = Owner.InternalChildren.IndexOf(before);
				Owner.InternalChildren.Insert(index, page);

				if (index == 0)
					Owner.RootPage = page;

				Owner.SendHandlerUpdate(false);
			}

			protected async override Task<Page> OnPopAsync(bool animated)
			{
				if (Owner.InternalChildren.Count == 1)
				{
					return null;
				}

				var currentPage = NavigationStack[NavigationStack.Count - 1];
				var newCurrentPage = NavigationStack[NavigationStack.Count - 2];

				await Owner.SendHandlerUpdateAsync(animated,
					() =>
					{
						Owner.RemoveFromInnerChildren(currentPage);
						Owner.CurrentPage = newCurrentPage;
					},
					() =>
					{
						Owner.SendNavigating(currentPage);
						Owner.FireDisappearing(currentPage);
						Owner.FireAppearing(newCurrentPage);
					},
					() =>
					{
						Owner.SendNavigated(currentPage);
						Owner?.Popped?.Invoke(Owner, new NavigationEventArgs(currentPage));
					});

				return currentPage;
			}

			protected override Task OnPopToRootAsync(bool animated)
			{
				if (NavigationStack.Count == 1)
					return Task.CompletedTask;

				Page previousPage = Owner.CurrentPage;
				Page newPage = Owner.RootPage;
				List<Page> pagesToRemove = new List<Page>();

				return Owner.SendHandlerUpdateAsync(animated,
					() =>
					{
						var lastIndex = NavigationStack.Count - 1;
						while (lastIndex > 0)
						{
							var page = (Page)NavigationStack[lastIndex];
							Owner.RemoveFromInnerChildren(page);
							lastIndex = NavigationStack.Count - 1;
							pagesToRemove.Insert(0, page);
						}
						Owner.CurrentPage = newPage;
					},
					() =>
					{
						Owner.SendNavigating(previousPage);
						Owner.FireDisappearing(previousPage);
						Owner.FireAppearing(newPage);
					},
					() =>
					{
						Owner.SendNavigated(previousPage);
						Owner?.PoppedToRoot?.Invoke(Owner, new PoppedToRootEventArgs(newPage, pagesToRemove));
					});
			}

			protected override Task OnPushAsync(Page root, bool animated)
			{
				if (Owner.InternalChildren.Contains(root))
					return Task.CompletedTask;

				var previousPage = Owner.CurrentPage;

				return Owner.SendHandlerUpdateAsync(animated,
					() =>
					{
						Owner.PushPage(root);
					},
					() =>
					{
						Owner.SendNavigating(previousPage);
						Owner.FireDisappearing(previousPage);
						Owner.FireAppearing(root);
					},
					() =>
					{
						Owner.SendNavigated(previousPage);
						Owner?.Pushed?.Invoke(Owner, new NavigationEventArgs(root));
					});
			}

			protected override void OnRemovePage(Page page)
			{
				if (page == null)
					throw new ArgumentNullException($"{nameof(page)} cannot be null.");

				if (page == Owner.CurrentPage && Owner.CurrentPage == Owner.RootPage)
					throw new InvalidOperationException("Cannot remove root page when it is also the currently displayed page.");

				if (page == Owner.CurrentPage)
				{
					Application.Current?.FindMauiContext()?.CreateLogger<NavigationPage>()?.LogWarning("RemovePage called for CurrentPage object. This can result in undesired behavior, consider calling PopAsync instead.");
					PopAsync();
					return;
				}

				if (!Owner.InternalChildren.Contains(page))
					throw new ArgumentException("Page to remove must be contained on this Navigation Page");

				Owner.RemoveFromInnerChildren(page);

				if (Owner.RootPage == page)
					Owner.RootPage = (Page)Owner.InternalChildren.First();


				Owner.SendHandlerUpdate(false);
			}
		}
	}
}
