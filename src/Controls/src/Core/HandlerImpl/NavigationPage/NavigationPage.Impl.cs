using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	public partial class NavigationPage : INavigationView
	{
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
			SyncToNavigationStack(newStack);
			var completionSource = _taskCompletionSource;
			CurrentPage = (Page)newStack[newStack.Count - 1];
			RootPage = (Page)newStack[0];
			CurrentNavigationTask = null;
			_taskCompletionSource = null;
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

		static void CurrentPagePropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var np = (NavigationPage)bindable;

			if (oldValue is INotifyPropertyChanged ncpOld)
			{
				ncpOld.PropertyChanged -= np.CurrentPagePropertyChanged;
			}

			if (newValue is INotifyPropertyChanged ncpNew)
			{
				ncpNew.PropertyChanged += np.CurrentPagePropertyChanged;
			}
		}

		void CurrentPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.IsOneOf(NavigationPage.HasNavigationBarProperty,
				NavigationPage.HasBackButtonProperty,
				NavigationPage.TitleIconImageSourceProperty,
				NavigationPage.TitleViewProperty,
				NavigationPage.IconColorProperty) ||
				e.IsOneOf(Page.TitleProperty, PlatformConfiguration.AndroidSpecific.AppCompat.NavigationPage.BarHeightProperty))
			{
				Handler?.UpdateValue(e.PropertyName);
			}
		}


		Task WaitForCurrentNavigationTask() =>
			CurrentNavigationTask ?? Task.CompletedTask;

		// This is used for navigation events that don't effect the currently visible page
		// InsertPageBefore/RemovePage
		async void SendHandlerUpdate(bool animated)
		{
			await WaitForCurrentNavigationTask();
			var trulyReadOnlyNavigationStack = new List<IView>(NavigationStack);
			var request = new NavigationRequest(trulyReadOnlyNavigationStack, animated);
			((INavigationView)this).RequestNavigation(request);
		}

		TaskCompletionSource<object> _taskCompletionSource;

		async Task SendHandlerUpdateAsync(bool animated, bool push = false, bool pop = false, bool popToRoot = false)
		{
			// Wait for any pending navigation tasks to finish
			await WaitForCurrentNavigationTask();

			var completionSource = new TaskCompletionSource<object>();
			CurrentNavigationTask = completionSource.Task;
			_taskCompletionSource = completionSource;

			// We create a new list to send to the handler because the structure backing 
			// The Navigation stack isn't immutable
			var previousPage = CurrentPage;
			var immutableNavigationStack = new List<IView>(NavigationStack);

			// Alert currently visible pages that navigation is happening
			SendNavigating();

			// Create the request for the handler
			var request = new NavigationRequest(immutableNavigationStack, animated);
			((INavigationView)this).RequestNavigation(request);

			// Wait for the handler to finish processing the navigation
			await completionSource.Task;

			// Send navigated event to currently visible pages and associated navigation event
			SendNavigated(previousPage);
			if (push)
				Pushed?.Invoke(this, new NavigationEventArgs(CurrentPage));
			else if (pop)
				Popped?.Invoke(this, new NavigationEventArgs(previousPage));
			else
				PoppedToRoot?.Invoke(this, new NavigationEventArgs(previousPage));
		}

		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();
			var immutableNavigationStack = new List<IView>(NavigationStack);
			SendNavigating();
			var request = new NavigationRequest(immutableNavigationStack, false);
			((INavigationView)this).RequestNavigation(request);
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


			void SendHandlerUpdate(bool animated)
			{
				var request = new NavigationRequest(GetNavigationStack(), false);
				Owner.Handler?.Invoke(nameof(INavigationView.RequestNavigation), request);
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

				SendHandlerUpdate(false);
			}

			protected async override Task<Page> OnPopAsync(bool animated)
			{
				var page = (Page)Owner.InternalChildren.Last();
				Owner.FireDisappearing(page);

				if (Owner.InternalChildren.Last() == page)
					Owner.FireAppearing((Page)Owner.InternalChildren[Owner.InternalChildren.Count - 2]);

				Owner.RemoveFromInnerChildren(page);
				Owner.CurrentPage = (Page)Owner.InternalChildren.Last();
				await Owner.SendHandlerUpdateAsync(animated, pop: true);
				return page;
			}

			protected override Task OnPopToRootAsync(bool animated)
			{
				Element[] childrenToRemove = Owner.InternalChildren.Skip(1).ToArray();
				foreach (Element child in childrenToRemove)
				{
					Owner.RemoveFromInnerChildren(child);
				}

				Owner.CurrentPage = Owner.RootPage;
				return Owner.SendHandlerUpdateAsync(animated, popToRoot: true);
			}

			protected override Task OnPushAsync(Page root, bool animated)
			{
				Owner.PushPage(root);
				return Owner.SendHandlerUpdateAsync(animated, push: true);
			}

			protected override void OnRemovePage(Page page)
			{
				if (page == null)
					throw new ArgumentNullException($"{nameof(page)} cannot be null.");

				if (page == Owner.CurrentPage && Owner.CurrentPage == Owner.RootPage)
					throw new InvalidOperationException("Cannot remove root page when it is also the currently displayed page.");

				if (page == Owner.CurrentPage)
				{
					Log.Warning("NavigationPage", "RemovePage called for CurrentPage object. This can result in undesired behavior, consider calling PopAsync instead.");
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