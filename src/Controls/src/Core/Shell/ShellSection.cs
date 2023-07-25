#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/Tab.xml" path="Type[@FullName='Microsoft.Maui.Controls.Tab']/Docs/*" />
	[EditorBrowsable(EditorBrowsableState.Always)]
	public class Tab : ShellSection
	{
	}

	/// <include file="../../../docs/Microsoft.Maui.Controls/ShellSection.xml" path="Type[@FullName='Microsoft.Maui.Controls.ShellSection']/Docs/*" />
	[ContentProperty(nameof(Items))]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public partial class ShellSection : ShellGroupItem, IShellSectionController, IPropertyPropagationController, IVisualTreeElement, IStackNavigation
	{
		#region PropertyKeys

		static readonly BindablePropertyKey ItemsPropertyKey =
			BindableProperty.CreateReadOnly(nameof(Items), typeof(ShellContentCollection), typeof(ShellSection), null,
				defaultValueCreator: bo => new ShellContentCollection() { Inner = new ElementCollection<ShellContent>(((ShellSection)bo).DeclaredChildren) });

		#endregion PropertyKeys

		#region IShellSectionController

		IShellSectionController ShellSectionController => this;
		readonly List<(object Observer, Action<Page> Callback)> _displayedPageObservers =
			new List<(object Observer, Action<Page> Callback)>();
		readonly List<IShellContentInsetObserver> _observers = new List<IShellContentInsetObserver>();
		Thickness _lastInset;
		double _lastTabThickness;

		event EventHandler<NavigationRequestedEventArgs> IShellSectionController.NavigationRequested
		{
			add { _navigationRequested += value; }
			remove { _navigationRequested -= value; }
		}

		event EventHandler<NavigationRequestedEventArgs> _navigationRequested;

		Page IShellSectionController.PresentedPage
		{
			get
			{
				if (Navigation.ModalStack.Count > 0)
				{
					if (Navigation.ModalStack[Navigation.ModalStack.Count - 1] is NavigationPage np)
						return np.Navigation.NavigationStack[np.Navigation.NavigationStack.Count - 1];

					return Navigation.ModalStack[0];
				}

				if (_navStack.Count > 1)
					return _navStack[_navStack.Count - 1];

				return ((IShellContentController)CurrentItem)?.Page;
			}
		}

		void IShellSectionController.AddContentInsetObserver(IShellContentInsetObserver observer)
		{
			if (!_observers.Contains(observer))
				_observers.Add(observer);

			observer.OnInsetChanged(_lastInset, _lastTabThickness);
		}

		void IShellSectionController.AddDisplayedPageObserver(object observer, Action<Page> callback)
		{
			_displayedPageObservers.Add((observer, callback));
			callback(DisplayedPage);
		}

		bool IShellSectionController.RemoveContentInsetObserver(IShellContentInsetObserver observer)
		{
			return _observers.Remove(observer);
		}

		bool IShellSectionController.RemoveDisplayedPageObserver(object observer)
		{
			foreach (var item in _displayedPageObservers)
			{
				if (item.Observer == observer)
				{
					return _displayedPageObservers.Remove(item);
				}
			}
			return false;
		}

		void IShellSectionController.SendInsetChanged(Thickness inset, double tabThickness)
		{
			foreach (var observer in _observers)
			{
				observer.OnInsetChanged(inset, tabThickness);
			}
			_lastInset = inset;
			_lastTabThickness = tabThickness;
		}

		async void IShellSectionController.SendPopping(Task poppingCompleted)
		{
			if (_navStack.Count <= 1)
				throw new Exception("Nav Stack consistency error");

			var page = _navStack[_navStack.Count - 1];

			_navStack.Remove(page);
			UpdateDisplayedPage();

			await poppingCompleted;

			RemovePage(page);

			(Parent?.Parent as IShellController)?.UpdateCurrentState(ShellNavigationSource.Pop);
		}

		async void IShellSectionController.SendPoppingToRoot(Task finishedPopping)
		{
			if (_navStack.Count <= 1)
				throw new Exception("Nav Stack consistency error");

			var oldStack = _navStack;
			_navStack = new List<Page> { null };

			for (int i = 1; i < oldStack.Count; i++)
				oldStack[i].SendDisappearing();

			UpdateDisplayedPage();
			await finishedPopping;

			for (int i = 1; i < oldStack.Count; i++)
				RemovePage(oldStack[i]);

			(Parent?.Parent as IShellController)?.UpdateCurrentState(ShellNavigationSource.PopToRoot);
		}

		[Obsolete]
		[EditorBrowsable(EditorBrowsableState.Never)]

		void IShellSectionController.SendPopped()
		{
			if (_navStack.Count <= 1)
				throw new Exception("Nav Stack consistency error");

			var last = _navStack[_navStack.Count - 1];
			_navStack.Remove(last);

			RemovePage(last);
		}

		// we want the list returned from here to remain point in time accurate
		ReadOnlyCollection<ShellContent> IShellSectionController.GetItems() => ((ShellContentCollection)Items).VisibleItemsReadOnly;

		[Obsolete]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void IShellSectionController.SendPopping(Page page)
		{
			if (_navStack.Count <= 1)
				throw new Exception("Nav Stack consistency error");

			_navStack.Remove(page);
			SendAppearanceChanged();
		}

		[Obsolete]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void IShellSectionController.SendPopped(Page page)
		{
			if (_navStack.Contains(page))
				_navStack.Remove(page);

			RemovePage(page);
		}


		event NotifyCollectionChangedEventHandler IShellSectionController.ItemsCollectionChanged
		{
			add { ((ShellContentCollection)Items).VisibleItemsChanged += value; }
			remove { ((ShellContentCollection)Items).VisibleItemsChanged -= value; }
		}

		#endregion IShellSectionController

		#region IPropertyPropagationController
		void IPropertyPropagationController.PropagatePropertyChanged(string propertyName)
		{
			PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, this, ((IVisualTreeElement)this).GetVisualChildren());
		}
		#endregion

		/// <summary>Bindable property for <see cref="CurrentItem"/>.</summary>
		public static readonly BindableProperty CurrentItemProperty =
			BindableProperty.Create(nameof(CurrentItem), typeof(ShellContent), typeof(ShellSection), null, BindingMode.TwoWay,
				propertyChanged: OnCurrentItemChanged);

		/// <summary>Bindable property for <see cref="Items"/>.</summary>
		public static readonly BindableProperty ItemsProperty = ItemsPropertyKey.BindableProperty;

		Page _displayedPage;
		List<Page> _navStack = new List<Page> { null };
		internal bool IsPushingModalStack { get; private set; }
		internal bool IsPoppingModalStack { get; private set; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellSection.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public ShellSection()
		{
			((ShellElementCollection)Items).VisibleItemsChangedInternal += (_, args) =>
			{
				if (args.OldItems != null)
				{
					foreach (Element item in args.OldItems)
					{
						OnVisibleChildRemoved(item);
					}
				}

				if (args.NewItems != null)
				{
					foreach (Element item in args.NewItems)
					{
						OnVisibleChildAdded(item);
					}
				}

				SendStructureChanged();
			};

			Navigation = new NavigationImpl(this);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellSection.xml" path="//Member[@MemberName='CurrentItem']/Docs/*" />
		public ShellContent CurrentItem
		{
			get { return (ShellContent)GetValue(CurrentItemProperty); }
			set { SetValue(CurrentItemProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellSection.xml" path="//Member[@MemberName='Items']/Docs/*" />
		public IList<ShellContent> Items => (IList<ShellContent>)GetValue(ItemsProperty);
		internal override ShellElementCollection ShellElementCollection => (ShellElementCollection)Items;

		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellSection.xml" path="//Member[@MemberName='Stack']/Docs/*" />
		public IReadOnlyList<Page> Stack => _navStack;

		internal Page DisplayedPage
		{
			get { return _displayedPage; }
			set
			{
				if (_displayedPage == value)
					return;

				_displayedPage = value;

				foreach (var item in _displayedPageObservers)
					item.Callback(_displayedPage);
			}
		}

		Shell Shell => Parent?.Parent as Shell;

		ShellItem ShellItem => Parent as ShellItem;

		internal static ShellSection CreateFromShellContent(ShellContent shellContent)
		{
			if (shellContent.Parent != null)
			{
				var current = (ShellSection)shellContent.Parent;

				if (current.Items.Contains(shellContent))
					current.CurrentItem = shellContent;

				return current;
			}

			var shellSection = new ShellSection();

			var contentRoute = shellContent.Route;

			shellSection.Route = Routing.GenerateImplicitRoute(contentRoute);

			shellSection.Items.Add(shellContent);

			shellSection.SetBinding(TitleProperty, new Binding(nameof(Title), BindingMode.OneWay, source: shellContent));
			shellSection.SetBinding(IconProperty, new Binding(nameof(Icon), BindingMode.OneWay, source: shellContent));
			shellSection.SetBinding(FlyoutIconProperty, new Binding(nameof(FlyoutIcon), BindingMode.OneWay, source: shellContent));

			return shellSection;
		}

		internal static ShellSection CreateFromTemplatedPage(TemplatedPage page)
		{
			return CreateFromShellContent((ShellContent)page);
		}

		public static implicit operator ShellSection(ShellContent shellContent)
		{
			return CreateFromShellContent(shellContent);
		}

		public static implicit operator ShellSection(TemplatedPage page)
		{
			return (ShellSection)(ShellContent)page;
		}

		async Task PrepareCurrentStackForBeingReplaced(ShellNavigationRequest request, ShellRouteParameters queryData, IServiceProvider services, bool? animate, List<string> globalRoutes, bool isRelativePopping)
		{
			string route = "";
			List<Page> navStack = null;

			// Pop the stack down to where it no longer matches 
			if (request.StackRequest == ShellNavigationRequest.WhatToDoWithTheStack.ReplaceIt)
			{
				// If there's a visible Modal Stack then let's remove the pages under it that
				// are going to be popped so they never become visible and never fire OnAppearing
				if (Navigation.ModalStack.Count > 0)
				{
					var navStackCopy = new List<Page>(_navStack);
					for (int i = 1; i < navStackCopy.Count; i++)
					{
						var routeToRemove = Routing.GetRoute(navStackCopy[i]);
						if (i > globalRoutes.Count || routeToRemove != globalRoutes[i - 1])
						{
							OnRemovePage(navStackCopy[i]);
						}
					}
				}

				RemoveExcessPathsWithinTheRoute();


				// now that we've removed all the extra pages let's find the page on the stack that 
				// will be visible so we can push that as the first visible thing
				// If we already have a modal stack then we don't worry about doing this
				// Modal pages can't be selectively inserted/removed so this only matters when there are
				// no modal pages present
				if (Navigation.ModalStack.Count == 0)
				{
					List<Page> pagesToInsert = new List<Page>();
					for (int i = 0; i < globalRoutes.Count; i++)
					{
						bool isLast = i == globalRoutes.Count - 1;
						int navIndex = i + 1;
						// Routes match so don't do anything
						if (navIndex < _navStack.Count && Routing.GetRoute(_navStack[navIndex]) == globalRoutes[i])
						{
							continue;
						}

						var page = GetOrCreateFromRoute(globalRoutes[i], queryData, services, i == globalRoutes.Count - 1, false);
						if (IsModal(page))
						{
							await PushModalAsync(page, IsNavigationAnimated(page));
							break;
						}
						else if (!isLast && navIndex < _navStack.Count)
						{
							Navigation.InsertPageBefore(page, _navStack[navIndex]);
						}
						else
						{
							pagesToInsert.Add(page);
						}
					}

					await PushStackOfPages(pagesToInsert, animate);
					RemoveExcessPathsWithinTheRoute();
				}

				for (int i = 0; i < globalRoutes.Count; i++)
				{
					bool isLast = i == globalRoutes.Count - 1;
					route = globalRoutes[i];

					navStack = ShellNavigationManager.BuildFlattenedNavigationStack(_navStack, Navigation?.ModalStack);

					// if the navStack count is one that means there is nothing pushed
					if (navStack.Count == 1)
						break;

					Page navPage = navStack.Count > i + 1 ? navStack[i + 1] : null;

					if (navPage != null)
					{
						// if the routes don't match then pop this route off the stack
						int popCount = i + 1;

						if (Routing.GetRoute(navPage) == route)
						{
							// if the routes do match and this is the last in the loop
							// pop everything after this route
							popCount = i + 2;
							ShellNavigationManager.ApplyQueryAttributes(navPage, queryData, isLast, isRelativePopping);

							// If we're not on the last loop of the stack then continue
							// otherwise pop the rest of the stack
							if (!isLast)
								continue;
						}

						// This is the page that we will eventually get to once we've finished
						// modifying the existing navigation stack
						// So we want to fire appearing on it						
						navPage.SendAppearing();

						IsPoppingModalStack = true;

						while (navStack.Count > popCount && Navigation.ModalStack.Count > 0)
						{
							bool isAnimated = animate ?? IsNavigationAnimated(navStack[navStack.Count - 1]);
							if (Navigation.ModalStack.Contains(navStack[navStack.Count - 1]))
							{
								await PopModalAsync(isAnimated);
							}
							else if (Navigation.ModalStack.Count > 0)
							{
								await Navigation.ModalStack[Navigation.ModalStack.Count - 1].Navigation.PopAsync(isAnimated);
							}

							navStack = ShellNavigationManager.BuildFlattenedNavigationStack(_navStack, Navigation?.ModalStack);
						}

						while (_navStack.Count > popCount)
						{
							// Remove middle pages before doing a pop on the visible page so that the transition
							// is seamless
							if ((_navStack.Count - popCount) == 1)
							{
								bool isAnimated = animate ?? IsNavigationAnimated(_navStack[_navStack.Count - 1]);
								await OnPopAsync(isAnimated);
							}
							else
							{
								OnRemovePage(_navStack[_navStack.Count - 2]);
							}
						}

						navStack = ShellNavigationManager.BuildFlattenedNavigationStack(_navStack, Navigation?.ModalStack);

						IsPoppingModalStack = false;

						break;
					}
				}
			}

			void RemoveExcessPathsWithinTheRoute()
			{
				// locate middle routes that were removed
				// //route/page1/page2 => //route/page2
				// Let's just remove page1 instead of pop, pop, add
				for (int i = 0; i < globalRoutes.Count; i++)
				{
					int foundMatchAt = -1;
					for (int j = 1; j < _navStack.Count; j++)
					{
						if (Routing.GetRoute(_navStack[j]) == globalRoutes[i])
						{
							foundMatchAt = j;
							break;
						}
					}

					// If we found a matching route then let's remove all the middle pages
					for (int j = foundMatchAt - 1; j >= (i + 1); j--)
					{
						OnRemovePage(_navStack[j]);
					}
				}
			}
		}

		Page GetOrCreateFromRoute(string route, ShellRouteParameters queryData, IServiceProvider services, bool isLast, bool isPopping)
		{
			var content = Routing.GetOrCreateContent(route, services) as Page;
			if (content == null)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<ShellSection>()?.LogWarning("Failed to Create Content For: {route}", route);
			}

			ShellNavigationManager.ApplyQueryAttributes(content, queryData, isLast, isPopping);
			return content;
		}

		internal async Task GoToAsync(ShellNavigationRequest request, ShellRouteParameters queryData, IServiceProvider services, bool? animate, bool isRelativePopping)
		{
			List<string> globalRoutes = request.Request.GlobalRoutes;
			if (globalRoutes == null || globalRoutes.Count == 0)
			{
				if (_navStack.Count == 2)
					await OnPopAsync(animate ?? false);
				else
					await OnPopToRootAsync(animate ?? false);

				return;
			}

			await PrepareCurrentStackForBeingReplaced(request, queryData, services, animate, globalRoutes, isRelativePopping);

			List<Page> modalPageStacks = new List<Page>();
			List<Page> nonModalPageStacks = new List<Page>();
			var currentNavStack = ShellNavigationManager.BuildFlattenedNavigationStack(_navStack, Navigation?.ModalStack);

			// populate global routes and build modal stacks

			// If the currentNavStack is larger than _navStack then we have modal pages
			bool weveGoneTotalModal = currentNavStack.Count > _navStack.Count;
			int whereToStartNavigation = 0;

			if (request.StackRequest == ShellNavigationRequest.WhatToDoWithTheStack.ReplaceIt)
				whereToStartNavigation = currentNavStack.Count - 1;

			for (int i = whereToStartNavigation; i < globalRoutes.Count; i++)
			{
				bool isLast = i == globalRoutes.Count - 1;
				var content = GetOrCreateFromRoute(globalRoutes[i], queryData, services, isLast, false);
				if (content == null)
				{
					break;
				}

				weveGoneTotalModal = weveGoneTotalModal || IsModal(content);

				if (weveGoneTotalModal)
				{
					modalPageStacks.Add(content);
				}
				else
				{
					nonModalPageStacks.Add(content);
				}
			}

			// Check if we have an active Navigation Page
			NavigationPage activeModalNavigationPage = null;
			for (int i = Navigation.ModalStack.Count - 1; i >= 0; i--)
			{
				if (Navigation.ModalStack[i] is NavigationPage np)
				{
					activeModalNavigationPage = np;
					break;
				}
			}

			for (int i = 0; i < modalPageStacks.Count; i++)
			{
				bool isLast = i == modalPageStacks.Count - 1;
				var modalPage = modalPageStacks[i];
				bool isAnimated = animate ?? IsNavigationAnimated(modalPage);
				IsPushingModalStack = !isLast;

				if (modalPage is NavigationPage np)
				{
					await PushModalAsync(modalPage, isAnimated);
					activeModalNavigationPage = np;
				}
				else
				{
					if (activeModalNavigationPage != null)
						await activeModalNavigationPage.Navigation.PushAsync(modalPage, animate ?? IsNavigationAnimated(modalPage));
					else
						await PushModalAsync(modalPage, isAnimated);
				}
			}

			await PushStackOfPages(nonModalPageStacks, animate);
		}

		Task PopModalAsync(bool isAnimated)
		{
			if (Navigation is NavigationImpl shellSectionProxy)
				return shellSectionProxy.PopModalInnerAsync(isAnimated);

			return Navigation.PopModalAsync(isAnimated);
		}

		Task PushModalAsync(Page page, bool isAnimated)
		{
			if (Navigation is NavigationImpl shellSectionProxy)
				return shellSectionProxy.PushModalInnerAsync(page, isAnimated);

			return Navigation.PushModalAsync(page, isAnimated);
		}

		async Task PushStackOfPages(List<Page> pages, bool? animate)
		{
			for (int i = pages.Count - 1; i >= 0; i--)
			{
				bool isLast = i == pages.Count - 1;

				if (isLast)
				{
					bool isAnimated = animate ?? IsNavigationAnimated(pages[i]);
					await OnPushAsync(pages[i], isAnimated);
				}
				else
					Navigation.InsertPageBefore(pages[i], pages[i + 1]);
			}
		}

		bool IsModal(BindableObject bo)
		{
			return (Shell.GetPresentationMode(bo) & PresentationMode.Modal) == PresentationMode.Modal;
		}

		bool IsNavigationAnimated(BindableObject bo)
		{
			return (Shell.GetPresentationMode(bo) & PresentationMode.NotAnimated) != PresentationMode.NotAnimated;
		}

		internal void SendStructureChanged()
		{
			if (Parent?.Parent is Shell shell)
			{
				if (IsVisibleSection)
					shell.SendStructureChanged();

				shell.SendFlyoutItemsChanged();
			}
		}

		protected virtual IReadOnlyList<Page> GetNavigationStack() => _navStack;

		internal void UpdateDisplayedPage()
		{
			var stack = Stack;
			var previousPage = DisplayedPage;
			if (stack.Count > 1)
			{
				DisplayedPage = stack[stack.Count - 1];
			}
			else
			{
				IShellContentController currentItem = CurrentItem;
				DisplayedPage = currentItem?.Page;
			}

			if (previousPage != DisplayedPage)
			{
				previousPage?.SendDisappearing();
				PresentedPageAppearing();
				SendAppearanceChanged();
			}
		}

		protected override void OnParentSet()
		{
			base.OnParentSet();

			if (this.IsVisibleSection)
				SendAppearanceChanged();
		}

		protected override void OnChildAdded(Element child)
		{
			base.OnChildAdded(child);
			OnVisibleChildAdded(child);
		}

		protected override void OnChildRemoved(Element child, int oldLogicalIndex)
		{
			if (child is IShellContentController sc && (sc.Page?.IsPlatformEnabled == true))
			{
				sc.Page.PlatformEnabledChanged += WaitForRendererToGetRemoved;
				void WaitForRendererToGetRemoved(object s, EventArgs p)
				{
					sc.Page.PlatformEnabledChanged -= WaitForRendererToGetRemoved;
					base.OnChildRemoved(child, oldLogicalIndex);
				};
			}
			else
			{
				base.OnChildRemoved(child, oldLogicalIndex);
			}

			OnVisibleChildRemoved(child);
		}

		void OnVisibleChildAdded(Element child)
		{
			if (CurrentItem == null && ((IShellSectionController)this).GetItems().Contains(child))
				SetValueFromRenderer(CurrentItemProperty, child);

			if (CurrentItem != null)
				UpdateDisplayedPage();
		}

		void OnVisibleChildRemoved(Element child)
		{
			if (CurrentItem == child)
			{
				var contentItems = ShellSectionController.GetItems();
				if (contentItems.Count == 0)
				{
					ClearValue(CurrentItemProperty, specificity: SetterSpecificity.FromHandler);
				}
				else
				{
					SetValueFromRenderer(CurrentItemProperty, contentItems[0]);
				}
			}

			UpdateDisplayedPage();
		}

		void InvokeNavigationRequest(NavigationRequestedEventArgs args)
		{
			_navigationRequested?.Invoke(this, args);
		}

		protected virtual void OnInsertPageBefore(Page page, Page before)
		{
			var index = _navStack.IndexOf(before);
			if (index == -1)
				throw new ArgumentException("Page not found in nav stack");

			var stack = _navStack.ToList();
			stack.Insert(index, page);

			var allow = ((IShellController)Shell).ProposeNavigation(
				ShellNavigationSource.Insert,
				Parent as ShellItem,
				this,
				CurrentItem,
				stack,
				true
			);

			if (!allow)
				return;

			_navStack.Insert(index, page);
			AddPage(page);

			var args = new NavigationRequestedEventArgs(page, before, false)
			{
				RequestType = NavigationRequestType.Insert
			};

			InvokeNavigationRequest(args);
		}

		protected async virtual Task<Page> OnPopAsync(bool animated)
		{
			if (_navStack.Count <= 1)
				throw new InvalidOperationException("Can't pop last page off stack");

			List<Page> stack = _navStack.ToList();
			stack.Remove(stack.Last());
			var allow = ((IShellController)Shell).ProposeNavigation(
				ShellNavigationSource.Pop,
				Parent as ShellItem,
				this,
				CurrentItem,
				stack,
				true
			);

			if (!allow)
				return null;

			var page = _navStack[_navStack.Count - 1];
			var args = new NavigationRequestedEventArgs(page, animated)
			{
				RequestType = NavigationRequestType.Pop
			};

			PresentedPageDisappearing();
			_navStack.Remove(page);
			PresentedPageAppearing();

			InvokeNavigationRequest(args);
			if (args.Task != null)
				await args.Task;

			if (_handlerBasedNavigationCompletionSource?.Task != null)
				await _handlerBasedNavigationCompletionSource.Task;

			RemovePage(page);

			return page;
		}

		protected virtual async Task OnPopToRootAsync(bool animated)
		{
			if (_navStack.Count <= 1)
				return;

			var allow = ((IShellController)Shell).ProposeNavigation(
				ShellNavigationSource.PopToRoot,
				Parent as ShellItem,
				this,
				CurrentItem,
				null,
				true
			);

			if (!allow)
				return;

			var page = _navStack[_navStack.Count - 1];
			var args = new NavigationRequestedEventArgs(page, animated)
			{
				RequestType = NavigationRequestType.PopToRoot
			};

			InvokeNavigationRequest(args);
			var oldStack = _navStack;
			_navStack = new List<Page> { null };

			if (args.Task != null)
				await args.Task;

			if (_handlerBasedNavigationCompletionSource?.Task != null)
				await _handlerBasedNavigationCompletionSource.Task;

			for (int i = 1; i < oldStack.Count; i++)
			{
				oldStack[i].SendDisappearing();
				RemovePage(oldStack[i]);
			}

			PresentedPageAppearing();
		}

		protected virtual Task OnPushAsync(Page page, bool animated)
		{
			List<Page> stack = _navStack.ToList();
			stack.Add(page);
			var allow = ((IShellController)Shell).ProposeNavigation(
				ShellNavigationSource.Push,
				ShellItem,
				this,
				CurrentItem,
				stack,
				true
			);

			if (!allow)
				return Task.FromResult(true);

			var args = new NavigationRequestedEventArgs(page, animated)
			{
				RequestType = NavigationRequestType.Push
			};

			PresentedPageDisappearing();
			_navStack.Add(page);
			PresentedPageAppearing();
			AddPage(page);
			InvokeNavigationRequest(args);

			return args.Task ??
				_handlerBasedNavigationCompletionSource?.Task ??
				Task.CompletedTask;
		}

		internal async Task PopModalStackToPage(Page page, bool? animated)
		{
			try
			{
				IsPoppingModalStack = true;
				int modalStackCount = Navigation.ModalStack.Count;
				for (int i = 0; i < modalStackCount; i++)
				{
					var pageToPop = Navigation.ModalStack[Navigation.ModalStack.Count - 1];
					if (pageToPop == page)
						break;

					// indicate that we are done popping down the stack to the modal page requested
					// This is mainly used by life cycle events so they don't fire onappearing
					if (page == null && Navigation.ModalStack.Count == 1)
					{
						IsPoppingModalStack = false;
					}
					else if (Navigation.ModalStack.Count > 1 && Navigation.ModalStack[Navigation.ModalStack.Count - 2] == page)
					{
						IsPoppingModalStack = false;
					}

					bool isAnimated = animated ?? (Shell.GetPresentationMode(pageToPop) & PresentationMode.NotAnimated) != PresentationMode.NotAnimated;
					await PopModalAsync(isAnimated);
				}
			}
			finally
			{
				IsPoppingModalStack = false;
			}
		}

		protected virtual void OnRemovePage(Page page)
		{
			if (!_navStack.Contains(page))
				return;

			bool currentPage = (((IShellSectionController)this).PresentedPage) == page;
			var stack = _navStack.ToList();
			stack.Remove(page);
			var allow = (!currentPage) ? true :
				((IShellController)Shell).ProposeNavigation(
					ShellNavigationSource.Remove,
					ShellItem,
					this,
					CurrentItem,
					stack,
					true
				);

			if (!allow)
				return;

			if (currentPage)
				PresentedPageDisappearing();

			_navStack.Remove(page);

			if (currentPage)
				PresentedPageAppearing();

			RemovePage(page);
			var args = new NavigationRequestedEventArgs(page, false)
			{
				RequestType = NavigationRequestType.Remove
			};
			InvokeNavigationRequest(args);
		}

		internal bool IsVisibleSection => Parent?.Parent is Shell shell && shell.CurrentItem?.CurrentItem == this;
		void PresentedPageDisappearing()
		{
			if (this is IShellSectionController sectionController)
			{
				CurrentItem?.SendDisappearing();
				sectionController.PresentedPage?.SendDisappearing();
			}
		}

		void PresentedPageAppearing()
		{
			if (IsVisibleSection && this is IShellSectionController sectionController)
			{
				if (_navStack.Count == 1)
					CurrentItem?.SendAppearing();

				var presentedPage = sectionController.PresentedPage;
				if (presentedPage != null)
				{
					if (presentedPage.Parent == null)
					{
						presentedPage.ParentSet += OnPresentedPageParentSet;

						void OnPresentedPageParentSet(object sender, EventArgs e)
						{
							PresentedPageAppearing();
							(sender as Page).ParentSet -= OnPresentedPageParentSet;
						}
					}
					else
					{

						this.FindParentOfType<Shell>().SendPageAppearing(presentedPage);
					}
				}
			}
		}

		static void OnCurrentItemChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var shellSection = (ShellSection)bindable;

			if (oldValue is ShellContent oldShellItem)
				oldShellItem.SendDisappearing();

			if (newValue == null)
				return;

			shellSection.PresentedPageAppearing();

			if (shellSection.Parent?.Parent is IShellController shell && shellSection.IsVisibleSection)
			{
				shell.UpdateCurrentState(ShellNavigationSource.ShellContentChanged);
			}

			shellSection.SendStructureChanged();

			if (shellSection.IsVisibleSection)
				((IShellController)shellSection?.Parent?.Parent)?.AppearanceChanged(shellSection, false);

			shellSection.UpdateDisplayedPage();
		}

		void AddPage(Page page)
		{
			AddLogicalChild(page);
		}

		void RemovePage(Page page)
		{
			RemoveLogicalChild(page);
		}

		void SendAppearanceChanged() => ((IShellController)Parent?.Parent)?.AppearanceChanged(this, false);

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			foreach (ShellContent shellContent in Items)
			{
				SetInheritedBindingContext(shellContent, BindingContext);
			}
		}

		internal override void SendDisappearing()
		{
			base.SendDisappearing();
			PresentedPageDisappearing();
		}

		internal override void SendAppearing()
		{
			base.SendAppearing();
			PresentedPageAppearing();
		}

		class NavigationImpl : NavigationProxy
		{
			readonly ShellSection _owner;

			public NavigationImpl(ShellSection owner) => _owner = owner;

			protected override IReadOnlyList<Page> GetNavigationStack() => _owner.GetNavigationStack();

			protected override async Task<Page> OnPopAsync(bool animated)
			{
				if (!_owner.IsVisibleSection)
				{
					return (await _owner.OnPopAsync(animated));
				}

				var navigationParameters = new ShellNavigationParameters()
				{
					Animated = animated,
					TargetState = new ShellNavigationState("..")
				};

				var returnedPage = (_owner as IShellSectionController).PresentedPage;
				await _owner.Shell.NavigationManager.GoToAsync(navigationParameters);

				// This means the page wasn't popped and navigation was cancelled
				if ((_owner as IShellSectionController).PresentedPage == returnedPage)
					return null;

				return returnedPage;
			}

			protected override Task OnPopToRootAsync(bool animated)
			{
				if (!_owner.IsVisibleSection)
				{
					return _owner.OnPopToRootAsync(animated);
				}

				var shell = _owner.Shell;
				var targetState =
					ShellNavigationManager.GetNavigationState(
						shell.CurrentItem,
						_owner,
						_owner.CurrentItem,
						null,
						null);

				var navigationParameters = new ShellNavigationParameters()
				{
					Animated = animated,
					TargetState = targetState,
					PopAllPagesNotSpecifiedOnTargetState = true
				};

				return _owner.Shell.NavigationManager.GoToAsync(navigationParameters);
			}

			protected override Task OnPushAsync(Page page, bool animated)
			{
				if (!_owner.IsVisibleSection)
					return _owner.OnPushAsync(page, animated);

				var navigationParameters = new ShellNavigationParameters()
				{
					Animated = animated,
					PagePushing = page
				};

				return _owner.Shell.NavigationManager.GoToAsync(navigationParameters);
			}

			// This is used when we just want to process the modal operation and we don't need
			// it to process through the internal shell navigation bits
			internal Task PushModalInnerAsync(Page modal, bool animated)
			{
				return Inner?.PushModalAsync(modal, animated);
			}

			// This is used when we just want to process the modal operation and we don't need
			// it to process through the internal shell navigation bits
			internal Task<Page> PopModalInnerAsync(bool animated)
			{
				return Inner?.PopModalAsync(animated);
			}

			protected override async Task OnPushModal(Page modal, bool animated)
			{
				if (_owner.Shell is null ||
					_owner.Shell.NavigationManager.AccumulateNavigatedEvents)
				{
					await base.OnPushModal(modal, animated);
					return;
				}

				if (animated)
					Shell.SetPresentationMode(modal, PresentationMode.ModalAnimated);
				else
					Shell.SetPresentationMode(modal, PresentationMode.ModalNotAnimated);

				var navigationParameters = new ShellNavigationParameters()
				{
					Animated = animated,
					PagePushing = modal
				};

				await _owner.Shell.NavigationManager.GoToAsync(navigationParameters);
			}

			protected async override Task<Page> OnPopModal(bool animated)
			{
				if (_owner.Shell.NavigationManager.AccumulateNavigatedEvents)
					return await base.OnPopModal(animated);

				var page = ModalStack[ModalStack.Count - 1];
				await _owner.Shell.GoToAsync("..", animated);
				return page;
			}

			protected override void OnRemovePage(Page page)
			{
				if (!_owner.IsVisibleSection || _owner.Shell.NavigationManager.AccumulateNavigatedEvents)
				{
					_owner.OnRemovePage(page);
					return;
				}

				var stack = _owner.Stack.ToList();
				stack.Remove(page);
				var navigationState = GetUpdatedStatus(stack);

				ShellNavigatingEventArgs shellNavigatingEventArgs =
					new ShellNavigatingEventArgs(
						_owner.Shell.CurrentState,
						navigationState.Location,
						ShellNavigationSource.Remove,
						false
					);

				_owner.Shell.NavigationManager.HandleNavigating(shellNavigatingEventArgs);
				_owner.OnRemovePage(page);
				(_owner.Shell as IShellController).UpdateCurrentState(ShellNavigationSource.Remove);
			}

			protected override void OnInsertPageBefore(Page page, Page before)
			{
				if (!_owner.IsVisibleSection || _owner.Shell.NavigationManager.AccumulateNavigatedEvents)
				{
					_owner.OnInsertPageBefore(page, before);
					return;
				}

				var stack = _owner.Stack.ToList();
				var index = stack.IndexOf(before);
				if (index == -1)
					throw new ArgumentException("Page not found in nav stack");

				stack.Insert(index, page);
				var navigationState = GetUpdatedStatus(stack);

				ShellNavigatingEventArgs shellNavigatingEventArgs =
					new ShellNavigatingEventArgs(
						_owner.Shell.CurrentState,
						navigationState.Location,
						ShellNavigationSource.Insert,
						false
					);

				_owner.Shell.NavigationManager.HandleNavigating(shellNavigatingEventArgs);
				_owner.OnInsertPageBefore(page, before);
				(_owner.Shell as IShellController).UpdateCurrentState(ShellNavigationSource.Insert);
			}

			ShellNavigationState GetUpdatedStatus(IReadOnlyList<Page> stack)
			{
				var shellItem = _owner.Shell.CurrentItem;
				var shellSection = shellItem?.CurrentItem;
				var shellContent = shellSection?.CurrentItem;
				var modalStack = shellSection?.Navigation?.ModalStack;
				return ShellNavigationManager.GetNavigationState(shellItem, shellSection, shellContent, stack, modalStack);
			}
		}

#nullable enable
		// This code only runs for shell bits that are running through a proper
		// ShellHandler
		TaskCompletionSource<object>? _handlerBasedNavigationCompletionSource;
		internal Task? PendingNavigationTask => _handlerBasedNavigationCompletionSource?.Task;

		void IStackNavigation.RequestNavigation(NavigationRequest eventArgs)
		{
			if (_handlerBasedNavigationCompletionSource != null)
				throw new InvalidOperationException("Pending Navigations still processing");

			_handlerBasedNavigationCompletionSource = new TaskCompletionSource<object>();
			Handler.Invoke(nameof(IStackNavigation.RequestNavigation), eventArgs);
		}

		void IStackNavigation.NavigationFinished(IReadOnlyList<IView> newStack)
		{
			_ = _handlerBasedNavigationCompletionSource ?? throw new InvalidOperationException("Mismatched Navigation finished");
			var source = _handlerBasedNavigationCompletionSource;
			_handlerBasedNavigationCompletionSource = null;
			source?.SetResult(true);
		}
#nullable disable

	}
}
