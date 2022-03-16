#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	[ContentProperty(nameof(Page))]
	public partial class Window : NavigableElement, IWindow, IVisualTreeElement, IToolbarElement, IMenuBarElement, IFlowDirectionController, IWindowController
	{
		public static readonly BindableProperty TitleProperty = BindableProperty.Create(
			nameof(Title), typeof(string), typeof(Window), default(string?));

		public static readonly BindableProperty PageProperty = BindableProperty.Create(
			nameof(Page), typeof(Page), typeof(Window), default(Page?),
			propertyChanged: OnPageChanged);

		public static readonly BindableProperty FlowDirectionProperty = BindableProperty.Create(nameof(FlowDirection), typeof(FlowDirection), typeof(Window), FlowDirection.MatchParent, propertyChanging: FlowDirectionChanging, propertyChanged: FlowDirectionChanged);

		HashSet<IWindowOverlay> _overlays = new HashSet<IWindowOverlay>();
		ReadOnlyCollection<Element>? _logicalChildren;
		List<IVisualTreeElement> _visualChildren;
		Toolbar? _toolbar;
		MenuBarTracker _menuBarTracker;

		IToolbar? IToolbarElement.Toolbar => Toolbar;
		internal Toolbar? Toolbar
		{
			get => _toolbar; set
			{
				_toolbar = value;
				Handler?.UpdateValue(nameof(IToolbarElement.Toolbar));
			}
		}

		public IReadOnlyCollection<IWindowOverlay> Overlays => _overlays.ToList().AsReadOnly();

		public IVisualDiagnosticsOverlay VisualDiagnosticsOverlay { get; }

		public Window()
		{
			_visualChildren = new List<IVisualTreeElement>();
			AlertManager = new AlertManager(this);
			ModalNavigationManager = new ModalNavigationManager(this);
			Navigation = new NavigationImpl(this);
			InternalChildren.CollectionChanged += OnCollectionChanged;
			VisualDiagnosticsOverlay = new VisualDiagnosticsOverlay(this);
			_menuBarTracker = new MenuBarTracker(this, "MenuBar");
		}

		public Window(Page page)
			: this()
		{
			Page = page;
		}

		IMenuBar? IMenuBarElement.MenuBar => _menuBarTracker.MenuBar;

		public string? Title
		{
			get => (string?)GetValue(TitleProperty);
			set => SetValue(TitleProperty, value);
		}

		public Page? Page
		{
			get => (Page?)GetValue(PageProperty);
			set => SetValue(PageProperty, value);
		}

		public event EventHandler<ModalPoppedEventArgs>? ModalPopped;
		public event EventHandler<ModalPoppingEventArgs>? ModalPopping;
		public event EventHandler<ModalPushedEventArgs>? ModalPushed;
		public event EventHandler<ModalPushingEventArgs>? ModalPushing;
		public event EventHandler? PopCanceled;

		public event EventHandler? Created;
		public event EventHandler? Resumed;
		public event EventHandler? Activated;
		public event EventHandler? Deactivated;
		public event EventHandler? Stopped;
		public event EventHandler? Destroying;
		public event EventHandler<BackgroundingEventArgs>? Backgrounding;
		public event EventHandler<DisplayDensityChangedEventArgs>? DisplayDensityChanged;

		protected virtual void OnCreated() { }
		protected virtual void OnResumed() { }
		protected virtual void OnActivated() { }
		protected virtual void OnDeactivated() { }
		protected virtual void OnStopped() { }
		protected virtual void OnDestroying() { }
		protected virtual void OnBackgrounding(IPersistedState state) { }
		protected virtual void OnDisplayDensityChanged(float displayDensity) { }	

		protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == nameof(Page))
				Handler?.UpdateValue(nameof(IWindow.Content));
		}

		/// <inheritdoc/>
		public bool AddOverlay(IWindowOverlay overlay)
		{
			if (overlay is IVisualDiagnosticsOverlay)
				return false;

			// Add the overlay. If it's added, 
			// Initalize the native layer if it wasn't already,
			// and call invalidate so it will be drawn.
			var result = _overlays.Add(overlay);
			if (result)
			{
				overlay.Initialize();
				overlay.Invalidate();
			}

			return result;
		}

		/// <inheritdoc/>
		public bool RemoveOverlay(IWindowOverlay overlay)
		{
			if (overlay is IVisualDiagnosticsOverlay)
				return false;

			var result = _overlays.Remove(overlay);
			if (result)
				overlay.Deinitialize();

			return result;
		}

		internal ObservableCollection<Element> InternalChildren { get; } = new ObservableCollection<Element>();

		internal override IReadOnlyList<Element> LogicalChildrenInternal =>
			_logicalChildren ??= new ReadOnlyCollection<Element>(InternalChildren);

		internal AlertManager AlertManager { get; }

		internal ModalNavigationManager ModalNavigationManager { get; }

		internal IMauiContext MauiContext =>
			Handler?.MauiContext ?? throw new InvalidOperationException("MauiContext is null.");

		IFlowDirectionController FlowController => this;

		public FlowDirection FlowDirection
		{
			get { return (FlowDirection)GetValue(FlowDirectionProperty); }
			set { SetValue(FlowDirectionProperty, value); }
		}

		EffectiveFlowDirection _effectiveFlowDirection = default(EffectiveFlowDirection);
		EffectiveFlowDirection IFlowDirectionController.EffectiveFlowDirection
		{
			get => _effectiveFlowDirection;
			set => SetEffectiveFlowDirection(value, true);
		}

		double IFlowDirectionController.Width => (Page as VisualElement)?.Width ?? 0;

		void SetEffectiveFlowDirection(EffectiveFlowDirection value, bool fireFlowDirectionPropertyChanged)
		{
			if (value == _effectiveFlowDirection)
				return;

			_effectiveFlowDirection = value;

			if (fireFlowDirectionPropertyChanged)
				OnPropertyChanged(FlowDirectionProperty.PropertyName);

		}

		static void FlowDirectionChanging(BindableObject bindable, object oldValue, object newValue)
		{
			var self = (IFlowDirectionController)bindable;

			if (self.EffectiveFlowDirection.IsExplicit() && oldValue == newValue)
				return;

			var newFlowDirection = ((FlowDirection)newValue).ToEffectiveFlowDirection(isExplicit: true);
			self.EffectiveFlowDirection = newFlowDirection;
		}

		static void FlowDirectionChanged(BindableObject bindable, object oldValue, object newValue)
		{
			PropertyPropagationExtensions.PropagatePropertyChanged(
				FlowDirectionProperty.PropertyName,
				(Element)bindable,
				((IElementController)bindable).LogicalChildren);
		}

		bool IFlowDirectionController.ApplyEffectiveFlowDirectionToChildContainer => true;

		Window IWindowController.Window
		{
			get => this;
			set => throw new InvalidOperationException("A window cannot set a window.");
		}

		IView IWindow.Content =>
			Page ?? throw new InvalidOperationException("No page was set on the window.");

		Application? Application => Parent as Application;

		void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				for (var i = 0; i < e.OldItems.Count; i++)
				{
					var item = (Element?)e.OldItems[i];

					if (item != null)
						_visualChildren.Remove(item);

					OnChildRemoved(item, e.OldStartingIndex + i);
				}
			}

			if (e.NewItems != null)
			{
				foreach (Element item in e.NewItems)
				{
					_visualChildren.Add(item);
					OnChildAdded(item);
					// TODO once we have better life cycle events on pages 
					if (item is Page)
					{
						SendWindowAppearing();
					}
				}
			}
		}

		void SendWindowAppearing()
		{
			Page?.SendAppearing();
		}

		void OnModalPopped(Page modalPage)
		{
			int index = _visualChildren.IndexOf(modalPage);
			_visualChildren.Remove(modalPage);

			var args = new ModalPoppedEventArgs(modalPage);
			ModalPopped?.Invoke(this, args);
			Application?.NotifyOfWindowModalEvent(args);

			VisualDiagnostics.OnChildRemoved(this, modalPage, index);
		}

		bool OnModalPopping(Page modalPage)
		{
			var args = new ModalPoppingEventArgs(modalPage);
			ModalPopping?.Invoke(this, args);
			Application?.NotifyOfWindowModalEvent(args);
			return args.Cancel;
		}

		void OnModalPushed(Page modalPage)
		{
			_visualChildren.Add(modalPage);
			var args = new ModalPushedEventArgs(modalPage);
			ModalPushed?.Invoke(this, args);
			Application?.NotifyOfWindowModalEvent(args);
			VisualDiagnostics.OnChildAdded(this, modalPage);
		}

		void OnModalPushing(Page modalPage)
		{
			var args = new ModalPushingEventArgs(modalPage);
			ModalPushing?.Invoke(this, args);
			Application?.NotifyOfWindowModalEvent(args);
		}

		void OnPopCanceled()
		{
			PopCanceled?.Invoke(this, EventArgs.Empty);
		}

		void IWindow.Created()
		{
			Created?.Invoke(this, EventArgs.Empty);
			OnCreated();
			Application?.SendStart();
		}

		void IWindow.Activated()
		{
			Activated?.Invoke(this, EventArgs.Empty);
			OnActivated();
		}

		void IWindow.Deactivated()
		{
			Deactivated?.Invoke(this, EventArgs.Empty);
			OnDeactivated();
		}

		void IWindow.Stopped()
		{
			Stopped?.Invoke(this, EventArgs.Empty);
			OnStopped();
			Application?.SendSleep();
		}

		void IWindow.Destroying()
		{
			Destroying?.Invoke(this, EventArgs.Empty);
			OnDestroying();

			Application?.RemoveWindow(this);
		}

		void IWindow.Resumed()
		{
			Resumed?.Invoke(this, EventArgs.Empty);
			OnResumed();
			Application?.SendResume();
		}

		void IWindow.Backgrounding(IPersistedState state)
		{
			Backgrounding?.Invoke(this, new BackgroundingEventArgs(state));
			OnBackgrounding(state);
		}

		void IWindow.DisplayDensityChanged(float displayDensity)
		{
			DisplayDensityChanged?.Invoke(this, new DisplayDensityChangedEventArgs(displayDensity));
			OnDisplayDensityChanged(displayDensity);
		}

		float IWindow.RequestDisplayDensity()
		{
			var request = new DisplayDensityRequest();
			var result = Handler?.InvokeWithResult(nameof(IWindow.RequestDisplayDensity), request);
			return result ?? 1.0f;
		}

		FlowDirection IWindow.FlowDirection
		{
			get
			{
				// If the user has set the root page to be RTL
				// Then we want the window to also reflect RTL
				// We don't want to force the user to reach into the window
				// in order to enable RTL Window features on WinUI
				if (FlowDirection == FlowDirection.MatchParent &&
					Page is IFlowDirectionController controller &&
					controller.EffectiveFlowDirection.IsExplicit())
				{
					return controller.EffectiveFlowDirection.ToFlowDirection();
				}

				return _effectiveFlowDirection.ToFlowDirection();
			}
		}

		public float DisplayDensity => ((IWindow)this).RequestDisplayDensity();

		private protected override void OnHandlerChangingCore(HandlerChangingEventArgs args)
		{
			base.OnHandlerChangingCore(args);
			var mauiContext = args?.NewHandler?.MauiContext;

			if (FlowDirection == FlowDirection.MatchParent && mauiContext != null)
			{
				FlowController.EffectiveFlowDirection = mauiContext.GetFlowDirection().ToEffectiveFlowDirection(true);
			}
		}

		// Currently this returns MainPage + ModalStack
		// Depending on how we want this to show up inside LVT
		// we might want to change this to only return the currently visible page
		IReadOnlyList<IVisualTreeElement> IVisualTreeElement.GetVisualChildren() =>
			_visualChildren;

		static void OnPageChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is not Window window)
				return;

			var oldPage = oldValue as Page;
			if (oldPage != null)
			{
				window.InternalChildren.Remove(oldPage);
				oldPage.HandlerChanged -= OnPageHandlerChanged;
				oldPage.HandlerChanging -= OnPageHandlerChanging;
			}

			var newPage = newValue as Page;
			if (newPage != null)
			{
				window.InternalChildren.Add(newPage);
				newPage.NavigationProxy.Inner = window.NavigationProxy;
				window._menuBarTracker.Target = newPage;
			}

			window.ModalNavigationManager.SettingNewPage();

			if (newPage != null)
			{
				newPage.HandlerChanged += OnPageHandlerChanged;
				newPage.HandlerChanging += OnPageHandlerChanging;

				if (newPage.Handler != null)
					OnPageHandlerChanged(newPage, EventArgs.Empty);
			}

			window?.Handler?.UpdateValue(nameof(IWindow.FlowDirection));

			void OnPageHandlerChanged(object? sender, EventArgs e)
			{
				window.ModalNavigationManager.PageAttachedHandler();
				window.AlertManager.Subscribe();
			}

			void OnPageHandlerChanging(object? sender, HandlerChangingEventArgs e)
			{
				window.AlertManager.Unsubscribe();
			}
		}

		bool IWindow.BackButtonClicked()
		{
			if (Navigation.ModalStack.Count > 0)
			{
				return Navigation.ModalStack[Navigation.ModalStack.Count - 1].SendBackButtonPressed();
			}

			return this.Page?.SendBackButtonPressed() ?? false;
		}

		class NavigationImpl : NavigationProxy
		{
			readonly Window _owner;

			public NavigationImpl(Window owner)
			{
				_owner = owner;
			}

			protected override IReadOnlyList<Page> GetModalStack()
			{
				return _owner.ModalNavigationManager.ModalStack;
			}

			protected override async Task<Page?> OnPopModal(bool animated)
			{
				Page modal = _owner.ModalNavigationManager.ModalStack[_owner.ModalNavigationManager.ModalStack.Count - 1];
				if (_owner.OnModalPopping(modal))
				{
					_owner.OnPopCanceled();
					return null;
				}

				Page? nextPage;
				if (modal.NavigationProxy.ModalStack.Count == 1)
				{
					nextPage = _owner.Page;
				}
				else
				{
					nextPage = _owner.ModalNavigationManager.ModalStack[_owner.ModalNavigationManager.ModalStack.Count - 2];
				}

				Page result = await _owner.ModalNavigationManager.PopModalAsync(animated);
				result.Parent = null;
				_owner.OnModalPopped(result);

				modal.SendNavigatedFrom(new NavigatedFromEventArgs(nextPage));
				nextPage?.SendNavigatedTo(new NavigatedToEventArgs(modal));

				return result;
			}

			protected override async Task OnPushModal(Page modal, bool animated)
			{
				_owner.OnModalPushing(modal);

				modal.Parent = _owner;

				if (modal.NavigationProxy.ModalStack.Count == 0)
				{
					modal.NavigationProxy.Inner = this;
					await _owner.ModalNavigationManager.PushModalAsync(modal, animated);
					_owner.Page?.SendNavigatedFrom(new NavigatedFromEventArgs(modal));
					modal.SendNavigatedTo(new NavigatedToEventArgs(_owner.Page));
				}
				else
				{
					var previousModalPage = modal.NavigationProxy.ModalStack[modal.NavigationProxy.ModalStack.Count - 1];
					await _owner.ModalNavigationManager.PushModalAsync(modal, animated);
					modal.NavigationProxy.Inner = this;
					previousModalPage.SendNavigatedFrom(new NavigatedFromEventArgs(modal));
					modal.SendNavigatedTo(new NavigatedToEventArgs(previousModalPage));
				}

				_owner.OnModalPushed(modal);
			}
		}
	}
}
