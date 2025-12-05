#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="Microsoft.Maui.Controls.Page"/> that manages the navigation and user-experience of a stack of other pages.</summary>
#if IOS || MACCATALYST
	[ElementHandler(typeof(Handlers.Compatibility.NavigationRenderer))]
#elif WINDOWS || ANDROID || TIZEN
	[ElementHandler(typeof(NavigationViewHandler))]
#endif
	public partial class NavigationPage : Page, IPageContainer<Page>, IBarElement, IElementConfiguration<NavigationPage>, IStackNavigationView, IToolbarElement
	{
		/// <summary>Bindable property for attached property <c>BackButtonTitle</c>.</summary>
		public static readonly BindableProperty BackButtonTitleProperty = BindableProperty.CreateAttached("BackButtonTitle", typeof(string), typeof(Page), null);

		/// <summary>Bindable property for attached property <c>HasNavigationBar</c>.</summary>
		public static readonly BindableProperty HasNavigationBarProperty =
			BindableProperty.CreateAttached("HasNavigationBar", typeof(bool), typeof(Page), true);

		/// <summary>Bindable property for attached property <c>HasBackButton</c>.</summary>
		public static readonly BindableProperty HasBackButtonProperty = BindableProperty.CreateAttached("HasBackButton", typeof(bool), typeof(NavigationPage), true);

		/// <summary>Bindable property for <see cref="BarBackgroundColor"/>.</summary>
		public static readonly BindableProperty BarBackgroundColorProperty = BarElement.BarBackgroundColorProperty;

		/// <summary>Bindable property for <see cref="BarBackground"/>.</summary>
		public static readonly BindableProperty BarBackgroundProperty = BarElement.BarBackgroundProperty;

		/// <summary>Bindable property for <see cref="BarTextColor"/>.</summary>
		public static readonly BindableProperty BarTextColorProperty = BarElement.BarTextColorProperty;

		/// <summary>Bindable property for attached property <c>TitleIconImageSource</c>.</summary>
		public static readonly BindableProperty TitleIconImageSourceProperty = BindableProperty.CreateAttached("TitleIconImageSource", typeof(ImageSource), typeof(NavigationPage), default(ImageSource));

		/// <summary>Bindable property for attached property <c>IconColor</c>.</summary>
		public static readonly BindableProperty IconColorProperty = BindableProperty.CreateAttached("IconColor", typeof(Color), typeof(NavigationPage), null);

		/// <summary>Bindable property for attached property <c>TitleView</c>.</summary>
		public static readonly BindableProperty TitleViewProperty = BindableProperty.CreateAttached("TitleView", typeof(View), typeof(NavigationPage), null,
			propertyChanging: TitleViewPropertyChanging, propertyChanged: (bo, oldV, newV) => bo.AddRemoveLogicalChildren(oldV, newV));

		static readonly BindablePropertyKey CurrentPagePropertyKey = BindableProperty.CreateReadOnly(nameof(CurrentPage), typeof(Page), typeof(NavigationPage), null, propertyChanged: OnCurrentPageChanged);

		/// <summary>Bindable property for <see cref="CurrentPage"/>.</summary>
		public static readonly BindableProperty CurrentPageProperty = CurrentPagePropertyKey.BindableProperty;

		static readonly BindablePropertyKey RootPagePropertyKey = BindableProperty.CreateReadOnly(nameof(RootPage), typeof(Page), typeof(NavigationPage), null);
		/// <summary>Bindable property for <see cref="RootPage"/>.</summary>
		public static readonly BindableProperty RootPageProperty = RootPagePropertyKey.BindableProperty;

		INavigationPageController NavigationPageController => this;


		partial void Init();

#if IOS || MACCATALYST
		const bool UseMauiHandler = false;
#else
		const bool UseMauiHandler = true;
#endif

		bool _setForMaui;

		/// <include file="../../docs/Microsoft.Maui.Controls/NavigationPage.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public NavigationPage() : this(UseMauiHandler)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/NavigationPage.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public NavigationPage(Page root) : this(UseMauiHandler, root)
		{
		}

		internal NavigationPage(bool setforMaui, Page root = null)
		{
			_setForMaui = setforMaui;
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<NavigationPage>>(() => new PlatformConfigurationRegistry<NavigationPage>(this));

			if (setforMaui)
				Navigation = new MauiNavigationImpl(this);
			else
				Navigation = new NavigationImpl(this);

			Init();

			if (root != null)
				PushPage(root);
		}

		/// <summary>Gets or sets the background color for the bar at the top of the NavigationPage. This is a bindable property.</summary>
		public Color BarBackgroundColor
		{
			get => (Color)GetValue(BarElement.BarBackgroundColorProperty);
			set => SetValue(BarElement.BarBackgroundColorProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/NavigationPage.xml" path="//Member[@MemberName='BarBackground']/Docs/*" />
		public Brush BarBackground
		{
			get => (Brush)GetValue(BarElement.BarBackgroundProperty);
			set => SetValue(BarElement.BarBackgroundProperty, value);
		}

		/// <summary>Gets or sets the text that appears on the bar at the top of the NavigationPage. This is a bindable property.</summary>
		public Color BarTextColor
		{
			get => (Color)GetValue(BarElement.BarTextColorProperty);
			set => SetValue(BarElement.BarTextColorProperty, value);
		}

		internal Task CurrentNavigationTask { get; set; }

		internal NavigationType NavigationType { get; set; }

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		/// <param name="depth">The depth parameter.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Page Peek(int depth)
		{
			if (depth < 0)
			{
				return null;
			}

			if (InternalChildren.Count <= depth)
			{
				return null;
			}

			return (Page)InternalChildren[InternalChildren.Count - depth - 1];
		}

		IEnumerable<Page> INavigationPageController.Pages => InternalChildren.Cast<Page>();

		int INavigationPageController.StackDepth
		{
			get { return InternalChildren.Count; }
		}

		/// <summary>The <see cref="Microsoft.Maui.Controls.Page"/> that is currently top-most on the navigation stack.</summary>
		public Page CurrentPage
		{
			get { return (Page)GetValue(CurrentPageProperty); }
			private set { SetValue(CurrentPagePropertyKey, value); }
		}

		/// <summary>The <see cref="Microsoft.Maui.Controls.Page"/> that is the root of the navigation stack.</summary>
		public Page RootPage
		{
			get { return (Page)GetValue(RootPageProperty); }
			private set { SetValue(RootPagePropertyKey, value); }
		}

		static void TitleViewPropertyChanging(BindableObject bindable, object oldValue, object newValue)
		{
			if (oldValue == newValue)
				return;

			if (bindable is Page page)
			{
				page.SetTitleView((View)oldValue, (View)newValue);
			}
		}

		/// <summary>The title of the back button for the specified <paramref name="page"/>.</summary>
		/// <param name="page">The <see cref="Microsoft.Maui.Controls.Page"/> whose back-button's title is being requested.</param>
		/// <returns>The title of the back button that would be shown if the specified <paramref name="page"/> were the <see cref="Microsoft.Maui.Controls.NavigationPage.CurrentPage"/>.</returns>
		public static string GetBackButtonTitle(BindableObject page)
		{
			return (string)page.GetValue(BackButtonTitleProperty);
		}

		/// <summary>Returns a value that indicates whether <paramref name="page"/> has a back button.</summary>
		/// <param name="page">The page parameter.</param>
		public static bool GetHasBackButton(Page page)
		{
			if (page == null)
				throw new ArgumentNullException(nameof(page));
			return (bool)page.GetValue(HasBackButtonProperty);
		}

		/// <summary>Returns a value that indicates whether the <paramref name="page"/> has a navigation bar.</summary>
		/// <param name="page">The <see cref="Microsoft.Maui.Controls.Page"/> being queried.</param>
		/// <returns><see langword="true"/> if <paramref name="page"/> would display a navigation bar were it the <see cref="Microsoft.Maui.Controls.NavigationPage.CurrentPage"/>.</returns>
		public static bool GetHasNavigationBar(BindableObject page)
		{
			return (bool)page.GetValue(HasNavigationBarProperty);
		}

		/// <param name="bindable">The bindable parameter.</param>
		public static ImageSource GetTitleIconImageSource(BindableObject bindable)
		{
			return (ImageSource)bindable.GetValue(TitleIconImageSourceProperty);
		}

		/// <summary>Returns the view to use as a title for the navigation page.</summary>
		/// <param name="bindable">The bindable object whose title view to get.</param>
		/// <returns>The view to use as a title for the navigation page.</returns>
		public static View GetTitleView(BindableObject bindable)
		{
			return (View)bindable.GetValue(TitleViewProperty);
		}

		/// <param name="bindable">The bindable parameter.</param>
		public static Color GetIconColor(BindableObject bindable)
		{
			if (bindable == null)
			{
				return null;
			}

			return (Color)bindable.GetValue(IconColorProperty);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/NavigationPage.xml" path="//Member[@MemberName='PopAsync'][1]/Docs/*" />
		public Task<Page> PopAsync()
		{
			return PopAsync(true);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/NavigationPage.xml" path="//Member[@MemberName='PopAsync'][2]/Docs/*" />
		public async Task<Page> PopAsync(bool animated)
		{
			// If Navigation interactions are being handled by the MAUI APIs
			// this routes the call there instead of through old behavior
			if (Navigation is MauiNavigationImpl mvi && this is IStackNavigation)
			{
				return await mvi.PopAsync(animated);
			}

			var tcs = new TaskCompletionSource<bool>();
			try
			{
				if (CurrentNavigationTask != null && !CurrentNavigationTask.IsCompleted)
				{
					var oldTask = CurrentNavigationTask;
					CurrentNavigationTask = tcs.Task;
					await oldTask;
				}
				else
					CurrentNavigationTask = tcs.Task;

				var result = await (this as INavigationPageController).PopAsyncInner(animated, false);
				tcs.SetResult(true);
				return result;
			}
			catch (Exception e)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<NavigationPage>()?.LogWarning(e, null);
				CurrentNavigationTask = null;
				tcs.SetCanceled();

				throw;
			}
		}

		public event EventHandler<NavigationEventArgs> Popped;

		public event EventHandler<NavigationEventArgs> PoppedToRoot;

		/// <include file="../../docs/Microsoft.Maui.Controls/NavigationPage.xml" path="//Member[@MemberName='PopToRootAsync'][1]/Docs/*" />
		public Task PopToRootAsync()
		{
			return PopToRootAsync(true);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/NavigationPage.xml" path="//Member[@MemberName='PopToRootAsync'][2]/Docs/*" />
		public async Task PopToRootAsync(bool animated)
		{
			// If Navigation interactions are being handled by the MAUI APIs
			// this routes the call there instead of through old behavior
			if (Navigation is MauiNavigationImpl mvi && this is IStackNavigation)
			{
				await mvi.PopToRootAsync(animated);
				return;
			}

			if (CurrentNavigationTask != null && !CurrentNavigationTask.IsCompleted)
			{
				var tcs = new TaskCompletionSource<bool>();
				Task oldTask = CurrentNavigationTask;
				CurrentNavigationTask = tcs.Task;
				await oldTask;

				await PopToRootAsyncInner(animated);
				tcs.SetResult(true);
				return;
			}

			Task result = PopToRootAsyncInner(animated);
			CurrentNavigationTask = result;
			await result;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/NavigationPage.xml" path="//Member[@MemberName='PushAsync'][1]/Docs/*" />
		public Task PushAsync(Page page)
		{
			return PushAsync(page, true);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/NavigationPage.xml" path="//Member[@MemberName='PushAsync'][2]/Docs/*" />
		public async Task PushAsync(Page page, bool animated)
		{
			// If Navigation interactions are being handled by the MAUI APIs
			// this routes the call there instead of through old behavior
			if (Navigation is MauiNavigationImpl mvi && this is IStackNavigation)
			{
				if (InternalChildren.Contains(page))
					return;

				await mvi.PushAsync(page, animated);
				return;
			}

			if (CurrentNavigationTask != null && !CurrentNavigationTask.IsCompleted)
			{
				var tcs = new TaskCompletionSource<bool>();
				Task oldTask = CurrentNavigationTask;
				CurrentNavigationTask = tcs.Task;
				await oldTask;

				await PushAsyncInner(page, animated);
				tcs.SetResult(true);
				return;
			}

			CurrentNavigationTask = PushAsyncInner(page, animated);
			await CurrentNavigationTask;
		}

		public event EventHandler<NavigationEventArgs> Pushed;

		/// <summary>Sets the title that appears on the back button for <paramref name="page"/>.</summary>
		/// <param name="page">The page parameter.</param>
		/// <param name="value">The value to set.</param>
		public static void SetBackButtonTitle(BindableObject page, string value)
		{
			page.SetValue(BackButtonTitleProperty, value);
		}

		/// <summary>Adds or removes a back button to <paramref name="page"/>, with optional animation.</summary>
		/// <param name="page">The page parameter.</param>
		/// <param name="value">The value to set.</param>
		public static void SetHasBackButton(Page page, bool value)
		{
			if (page == null)
				throw new ArgumentNullException(nameof(page));
			page.SetValue(HasBackButtonProperty, value);
		}

		/// <summary>Sets a value that indicates whether or not this <see cref="Microsoft.Maui.Controls.NavigationPage"/> element has a navigation bar.</summary>
		/// <param name="page">The page parameter.</param>
		/// <param name="value">The value to set.</param>
		public static void SetHasNavigationBar(BindableObject page, bool value)
		{
			page.SetValue(HasNavigationBarProperty, value);
		}

		/// <param name="bindable">The bindable parameter.</param>
		/// <param name="value">The value to set.</param>
		public static void SetTitleIconImageSource(BindableObject bindable, ImageSource value)
		{
			bindable.SetValue(TitleIconImageSourceProperty, value);
		}

		/// <summary>Sets the view to use as the title for the navigation page.</summary>
		/// <param name="bindable">The bindable object whose title to set.</param>
		/// <param name="value">The view to use.</param>
		public static void SetTitleView(BindableObject bindable, View value)
		{
			bindable.SetValue(TitleViewProperty, value);
		}

		/// <param name="bindable">The bindable parameter.</param>
		/// <param name="value">The value to set.</param>
		public static void SetIconColor(BindableObject bindable, Color value)
		{
			bindable.SetValue(IconColorProperty, value);
		}

		protected override bool OnBackButtonPressed()
		{
			if (CurrentPage.SendBackButtonPressed())
				return true;

			if (NavigationPageController.StackDepth > 1)
			{
				SafePop();
				return true;
			}

			return base.OnBackButtonPressed();
		}

		void SendNavigated(Page previousPage, NavigationType navigationType)
		{
			previousPage?.SendNavigatedFrom(new NavigatedFromEventArgs(CurrentPage, navigationType));
			CurrentPage.SendNavigatedTo(new NavigatedToEventArgs(previousPage, navigationType));
		}

		void SendNavigating(NavigationType navigationType, Page navigatingFrom = null)
		{
			(navigatingFrom ?? CurrentPage)?.SendNavigatingFrom(new NavigatingFromEventArgs(CurrentPage, navigationType));
		}


		void FireDisappearing(Page page)
		{
			if (HasAppeared)
				page?.SendDisappearing();
		}

		void FireAppearing(Page page)
		{
			if (HasAppeared)
				page?.SendAppearing();
		}


		void RemoveFromInnerChildren(Element page)
		{
			InternalChildren.Remove(page);
		}

		void SafePop()
		{
			PopAsync(true).ContinueWith(t =>
			{
				if (t.IsFaulted)
					throw t.Exception;
			});
		}

		readonly Lazy<PlatformConfigurationRegistry<NavigationPage>> _platformConfigurationRegistry;

		/// <inheritdoc/>
		public new IPlatformElementConfiguration<T, NavigationPage> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		// If the user is making overlapping navigation requests this is used to fire once all navigation 
		// events have been processed
		TaskCompletionSource<object> _allPendingNavigationCompletionSource;

		// This is used to process the currently active navigation request
		TaskCompletionSource<object> _currentNavigationCompletionSource;

		int _waitingCount = 0;
		NavigationPageToolbar _toolbar;
		readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);

		partial void Init()
		{
			(this as IControlsVisualElement).WindowChanged += OnWindowChanged;
		}

		Thickness IView.Margin => Thickness.Zero;

		[Obsolete("Use ArrangeOverride instead")]
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			// We don't want forcelayout to call the legacy
			// Page.LayoutChildren code
		}

		void IStackNavigation.RequestNavigation(NavigationRequest eventArgs)
		{
			Handler?.Invoke(nameof(IStackNavigation.RequestNavigation), eventArgs);
		}

		// If a native navigation occurs then this syncs up the NavigationStack
		// with the new native Navigation Stack
		void IStackNavigation.NavigationFinished(IReadOnlyList<IView> newStack)
		{
			// If the user is performing multiple overlapping navigations then we don't want to sync the native stack to our xplat stack
			// We wait until we get to the end of the queue and then we sync up.
			// Otherwise intermediate results will wipe out the navigationstack
			if (_waitingCount <= 1)
			{
				SyncToNavigationStack(newStack);
				CurrentPage = (Page)newStack[newStack.Count - 1];
				RootPage = (Page)newStack[0];
				FindMyToolbar()?.Handler?.UpdateValue(nameof(IToolbar.BackButtonVisible));
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

		internal IToolbar FindMyToolbar()
		{
			if (this.Toolbar != null)
				return Toolbar;

			if (this.Window is null)
				return null;

			var rootPage = this.FindParentWith(x => (x is IWindow te || Window.Navigation.ModalStack.Contains(x)), true);
			if (this.FindParentWith(x => (x is IToolbarElement te && te.Toolbar != null), false) is IToolbarElement te)
			{
				// This means I'm inside a Modal Page so we shouldn't return the Toolbar from the window
				if (rootPage is not IWindow && te is IWindow)
					return null;

				return te.Toolbar;
			}

			return null;
		}

		void OnWindowChanged(object sender, EventArgs e)
		{
			// If this NavigationPage is removed from the window
			// Then we just invalidate the toolbar that this NavigationPage created
			if (this.Window is null)
			{
				if (_toolbar is not null)
				{
					if (_toolbar.Parent is Window w &&
						w.Toolbar == _toolbar)
					{
						w.Toolbar = null;
					}

					var flyoutPage = _toolbar.FindParentOfType<FlyoutPage>();
					if (flyoutPage != null && flyoutPage.Parent is IWindow && flyoutPage.Toolbar == _toolbar)
					{
						flyoutPage.Toolbar = null;
					}

					_toolbar.Disconnect();
					_toolbar = null;
				}

				return;
			}

			// Update the Container level Toolbar with my Toolbar information
			if (FindMyToolbar() is not NavigationPageToolbar)
			{
				// If the root is a FlyoutPage then we set the toolbar on the flyout page
				var flyoutPage = this.FindParentOfType<FlyoutPage>();

				if (flyoutPage != null && flyoutPage.Parent is IWindow)
				{
					_toolbar = new NavigationPageToolbar(flyoutPage, flyoutPage);
					flyoutPage.Toolbar = _toolbar;
				}
				// Is the root a modal page?
				else
				{
					// Is the root the window or is this part of a modal stack
					Element toolbarRoot;

					var parentPages = this.GetParentPages();
					parentPages.Insert(0, this);
					var topLevelPage = parentPages[parentPages.Count - 1];

					// Is my top parent page the root page on the window?
					// If so then we set the toolbar on the window
					if (Window.Page == topLevelPage)
					{
						toolbarRoot = Window;
					}
					else
					{
						// This means the page is a modal page so we set the toolbar on the top level page
						// of the modal
						toolbarRoot = topLevelPage;
					}

					if (toolbarRoot is Window w)
					{
						_toolbar = new NavigationPageToolbar(w, w.Page);
						w.Toolbar = _toolbar;
					}
					else if (toolbarRoot is Page p)
					{
						_toolbar = new NavigationPageToolbar(p, p);
						p.Toolbar = _toolbar;
					}
				}
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

				// If our handler was removed while waiting then don't do anything
				if (Handler != null)
				{
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
					((IStackNavigation)this).RequestNavigation(request);

					// Wait for the handler to finish processing the navigation
					// This task completes once the handler calls INavigationView.Finished
					await currentNavRequestTaskSource.Task;
				}
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

			if (Navigation is MauiNavigationImpl && InternalChildren.Count > 0)
			{
				var navStack = Navigation.NavigationStack;
				var visiblePage = Navigation.NavigationStack[NavigationStack.Count - 1];
				RootPage = navStack[0];
				CurrentPage = visiblePage;

				var navigationType = DetermineNavigationType();

				SendHandlerUpdateAsync(false, null,
				() =>
				{
					FireAppearing(CurrentPage);
				},
				() =>
				{
					SendNavigated(null, navigationType);
				})
				.FireAndForget(Handler);
			}

			// If the handler is disconnected and we're still waiting for updates from the handler
			// Just complete any waits
			if (Handler == null && _waitingCount > 0)
			{
				((IStackNavigation)this).NavigationFinished(this.NavigationStack);
			}
		}

		NavigationType DetermineNavigationType()
		{
			var parentPages = this.GetParentPages();

			bool hasTabOrFlyout = parentPages.Any(page => page is FlyoutPage or TabbedPage);

			if (hasTabOrFlyout)
			{
				return NavigationType.Replace;
			}

			return NavigationType.Push;
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


				Owner.SendHandlerUpdateAsync(false,
					() =>
					{
						Owner.NavigationType = NavigationType.Insert;
						int index = Owner.InternalChildren.IndexOf(before);
						Owner.InternalChildren.Insert(index, page);

						if (index == 0)
							Owner.RootPage = page;
					},
					() =>
					{
					},
					() =>
					{
						//// If no other pending operations happen
						//// Then update the toolbar to match
						//// the current navigation stack
						//if (Owner._waitingCount == 0)
						//	Owner.UpdateToolbar();

					}).FireAndForget();
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
						Owner.SendNavigating(NavigationType.Pop, currentPage);
						Owner.FireDisappearing(currentPage);
						Owner.RemoveFromInnerChildren(currentPage);
						Owner.CurrentPage = newCurrentPage;
						Owner.RemoveFromInnerChildren(currentPage);
						if (currentPage.TitleView != null)
						{
							currentPage.RemoveLogicalChild(currentPage.TitleView);
						}
					},
					() =>
					{
						Owner.FireAppearing(newCurrentPage);
					},
					() =>
					{
						Owner.SendNavigated(currentPage, NavigationType.Pop);
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
						Owner.SendNavigating(NavigationType.PopToRoot, previousPage);
						Owner.FireDisappearing(previousPage);
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
						Owner.FireAppearing(newPage);
					},
					() =>
					{
						Owner.SendNavigated(previousPage, NavigationType.PopToRoot);
						Owner?.PoppedToRoot?.Invoke(Owner, new PoppedToRootEventArgs(newPage, pagesToRemove));
					});
			}

			protected override Task OnPushAsync(Page root, bool animated)
			{
				if (Owner.InternalChildren.Contains(root))
					return Task.CompletedTask;

				var navigationType = Owner.DetermineNavigationType();
				var previousPage = Owner.CurrentPage;

				return Owner.SendHandlerUpdateAsync(animated,
					() =>
					{
						Owner.NavigationType = navigationType;
						// Move the SendNavigating here so that it's fired prior to the stack being modified
						// This ensures consistent event ordering across all platforms (iOS, Catalyst, Android, Windows)
						Owner.SendNavigating(navigationType, previousPage);
						Owner.PushPage(root);
					},
					() =>
					{
						Owner.FireDisappearing(previousPage);
						Owner.FireAppearing(root);
					},
					() =>
					{
						Owner.SendNavigated(previousPage, navigationType);
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

				Owner.SendHandlerUpdateAsync(false,
					() =>
					{
						Owner.NavigationType = NavigationType.Remove;
						Owner.RemoveFromInnerChildren(page);

						if (Owner.RootPage == page)
							Owner.RootPage = (Page)Owner.InternalChildren[0];
					},
					() =>
					{
					},
					() =>
					{
						//// If no other pending operations happen
						//// Then update the toolbar to match
						//// the current navigation stack
						//if (Owner._waitingCount == 0)
						//	Owner.UpdateToolbar();

					}).FireAndForget();
			}
		}
	}
}
