#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="Type[@FullName='Microsoft.Maui.Controls.Page']/Docs/*" />
	public partial class Page : VisualElement, ILayout, IPageController, IElementConfiguration<Page>, IPaddingElement, ISafeAreaView, ISafeAreaView2, IView, ITitledElement, IToolbarElement
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='BusySetSignalName']/Docs/*" />
		public const string BusySetSignalName = "Microsoft.Maui.Controls.BusySet";

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='AlertSignalName']/Docs/*" />
		public const string AlertSignalName = "Microsoft.Maui.Controls.SendAlert";

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='PromptSignalName']/Docs/*" />
		public const string PromptSignalName = "Microsoft.Maui.Controls.SendPrompt";

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='ActionSheetSignalName']/Docs/*" />
		public const string ActionSheetSignalName = "Microsoft.Maui.Controls.ShowActionSheet";

		internal static readonly BindableProperty IgnoresContainerAreaProperty = BindableProperty.Create("IgnoresContainerArea", typeof(bool), typeof(Page), false);

		/// <summary>Bindable property for <see cref="BackgroundImageSource"/>.</summary>
		public static readonly BindableProperty BackgroundImageSourceProperty = BindableProperty.Create(nameof(BackgroundImageSource), typeof(ImageSource), typeof(Page), default(ImageSource));

		/// <summary>Bindable property for <see cref="IsBusy"/>.</summary>
		public static readonly BindableProperty IsBusyProperty = BindableProperty.Create("IsBusy", typeof(bool), typeof(Page), false, propertyChanged: (bo, o, n) => ((Page)bo).OnPageBusyChanged());

		/// <summary>Bindable property for <see cref="Padding"/>.</summary>
		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		/// <summary>Bindable property for <see cref="Title"/>.</summary>
		public static readonly BindableProperty TitleProperty = BindableProperty.Create("Title", typeof(string), typeof(Page), null);

		/// <summary>Bindable property for <see cref="IconImageSource"/>.</summary>
		public static readonly BindableProperty IconImageSourceProperty = BindableProperty.Create(nameof(IconImageSource), typeof(ImageSource), typeof(Page), default(ImageSource));

		readonly Lazy<PlatformConfigurationRegistry<Page>> _platformConfigurationRegistry;

		bool _allocatedFlag;
		Rect _containerArea;

		bool _containerAreaSet;

		bool _hasAppeared;
		private protected bool HasAppeared => _hasAppeared;

		View _titleView;

		List<Action> _pendingActions = new List<Action>();

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Page()
		{
			var toolbarItems = new ObservableCollection<ToolbarItem>();
			toolbarItems.CollectionChanged += OnToolbarItemsCollectionChanged;
			ToolbarItems = toolbarItems;

			var menuBarItems = new ObservableCollection<MenuBarItem>();
			menuBarItems.CollectionChanged += OnToolbarItemsCollectionChanged;
			MenuBarItems = menuBarItems;

			//if things were added in base ctor (through implicit styles), the items added aren't properly parented
			if (InternalChildren.Count > 0)
				InternalChildrenOnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, InternalChildren));

			InternalChildren.CollectionChanged += InternalChildrenOnCollectionChanged;
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Page>>(() => new PlatformConfigurationRegistry<Page>(this));
			this.NavigatedTo += FlushPendingActions;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='BackgroundImageSource']/Docs/*" />
		public ImageSource BackgroundImageSource
		{
			get { return (ImageSource)GetValue(BackgroundImageSourceProperty); }
			set { SetValue(BackgroundImageSourceProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='IconImageSource']/Docs/*" />
		public ImageSource IconImageSource
		{
			get { return (ImageSource)GetValue(IconImageSourceProperty); }
			set { SetValue(IconImageSourceProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='IsBusy']/Docs/*" />
		public bool IsBusy
		{
			get { return (bool)GetValue(IsBusyProperty); }
			set { SetValue(IsBusyProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='Padding']/Docs/*" />
		public Thickness Padding
		{
			get { return (Thickness)GetValue(PaddingElement.PaddingProperty); }
			set { SetValue(PaddingElement.PaddingProperty, value); }
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator()
		{
			return default(Thickness);
		}

		void IPaddingElement.OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue)
		{
			UpdateChildrenLayout();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='Title']/Docs/*" />
		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='ToolbarItems']/Docs/*" />
		public IList<ToolbarItem> ToolbarItems { get; internal set; }

		public IList<MenuBarItem> MenuBarItems { get; internal set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='ContainerArea']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Rect ContainerArea
		{
			get { return _containerArea; }
			set
			{
				if (_containerArea == value)
					return;
				_containerAreaSet = true;
				_containerArea = value;
				ForceLayout();
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='IgnoresContainerArea']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IgnoresContainerArea
		{
			get { return (bool)GetValue(IgnoresContainerAreaProperty); }
			set { SetValue(IgnoresContainerAreaProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='InternalChildren']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ObservableCollection<Element> InternalChildren { get; } = new ObservableCollection<Element>();

		// Todo rework Page to use AddLogical/RemoveLogical.
		// Rework all the related parts of the code that interact with the `Page` InternalChildren
		private protected override IList<Element> LogicalChildrenInternalBackingStore =>
			InternalChildren;

		internal override IEnumerable<Element> ChildrenNotDrawnByThisElement
		{
			get
			{
				var titleviewPart1TheShell = Shell.GetTitleView(this);
				var titleViewPart2TheNavBar = NavigationPage.GetTitleView(this);

				if (titleviewPart1TheShell != null)
					yield return titleviewPart1TheShell;

				if (titleViewPart2TheNavBar != null)
					yield return titleViewPart2TheNavBar;
			}
		}

		bool ISafeAreaView.IgnoreSafeArea => !On<PlatformConfiguration.iOS>().UsingSafeArea();

		Thickness ISafeAreaView2.SafeAreaInsets
		{
			set
			{
				On<PlatformConfiguration.iOS>().SetSafeAreaInsets(value);
			}
		}

		public event EventHandler LayoutChanged;

		public event EventHandler Appearing;

		public event EventHandler Disappearing;

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='DisplayActionSheet'][1]/Docs/*" />
		public Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons)
		{
			return DisplayActionSheet(title, cancel, destruction, FlowDirection.MatchParent, buttons);
		}

		public Task<string> DisplayActionSheet(string title, string cancel, string destruction, FlowDirection flowDirection, params string[] buttons)
		{
			var args = new ActionSheetArguments(title, cancel, destruction, buttons);

			args.FlowDirection = flowDirection;
#pragma warning disable CS0618 // TODO: Remove when we internalize/replace MessagingCenter
			if (IsPlatformEnabled)
				MessagingCenter.Send(this, ActionSheetSignalName, args);
			else
				_pendingActions.Add(() => MessagingCenter.Send(this, ActionSheetSignalName, args));
#pragma warning restore CS0618 // Type or member is obsolete

			return args.Result.Task;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='DisplayAlert'][1]/Docs/*" />
		public Task DisplayAlert(string title, string message, string cancel)
		{
			return DisplayAlert(title, message, null, cancel, FlowDirection.MatchParent);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='DisplayAlert'][2]/Docs/*" />
		public Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
		{
			return DisplayAlert(title, message, accept, cancel, FlowDirection.MatchParent);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='DisplayAlert'][1]/Docs/*" />
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
		public Task DisplayAlert(string title, string message, string cancel, FlowDirection flowDirection)
#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
		{
			return DisplayAlert(title, message, null, cancel, flowDirection);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='DisplayAlert'][2]/Docs/*" />
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
		public Task<bool> DisplayAlert(string title, string message, string accept, string cancel, FlowDirection flowDirection)
#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
		{
			if (string.IsNullOrEmpty(cancel))
				throw new ArgumentNullException("cancel");

			var args = new AlertArguments(title, message, accept, cancel);
			args.FlowDirection = flowDirection;

#pragma warning disable CS0618 // TODO: Remove when we internalize/replace MessagingCenter
			if (IsPlatformEnabled)
				MessagingCenter.Send(this, AlertSignalName, args);
			else
				_pendingActions.Add(() => MessagingCenter.Send(this, AlertSignalName, args));
#pragma warning restore CS0618 // Type or member is obsolete

			return args.Result.Task;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='DisplayPromptAsync'][2]/Docs/*" />
		public Task<string> DisplayPromptAsync(string title, string message, string accept = "OK", string cancel = "Cancel", string placeholder = null, int maxLength = -1, Keyboard keyboard = default(Keyboard), string initialValue = "")
		{
			var args = new PromptArguments(title, message, accept, cancel, placeholder, maxLength, keyboard, initialValue);

#pragma warning disable CS0618 // TODO: Remove when we internalize/replace MessagingCenter
			if (IsPlatformEnabled)
				MessagingCenter.Send(this, PromptSignalName, args);
			else
				_pendingActions.Add(() => MessagingCenter.Send(this, PromptSignalName, args));
#pragma warning restore CS0618 // Type or member is obsolete

			return args.Result.Task;
		}

		void FlushPendingActions(object sender, EventArgs e)
		{
			if (_pendingActions.Count > 0)
			{
				var actionsToProcess = _pendingActions.ToList();
				_pendingActions.Clear();
				foreach (var pendingAction in actionsToProcess)
					pendingAction();
			}

			this.NavigatedTo -= FlushPendingActions;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='ForceLayout']/Docs/*" />
		public void ForceLayout()
		{
			SizeAllocated(Width, Height);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='SendBackButtonPressed']/Docs/*" />
		public bool SendBackButtonPressed()
		{
			return OnBackButtonPressed();
		}

		protected virtual void LayoutChildren(double x, double y, double width, double height)
		{
			var area = new Rect(x, y, width, height);
			Rect originalArea = area;
			if (_containerAreaSet)
			{
				area = ContainerArea;
				area.X += Padding.Left;
				area.Y += Padding.Right;
				area.Width -= Padding.HorizontalThickness;
				area.Height -= Padding.VerticalThickness;
				area.Width = Math.Max(0, area.Width);
				area.Height = Math.Max(0, area.Height);
			}

			List<Element> elements = ((IElementController)this).LogicalChildren.ToList();
			foreach (Element element in elements)
			{
				var child = element as VisualElement;
				if (child == null)
					continue;

				var page = child as Page;
				if (page != null && page.IgnoresContainerArea)
					Maui.Controls.Compatibility.Layout.LayoutChildIntoBoundingRegion(child, originalArea);
				else
					Maui.Controls.Compatibility.Layout.LayoutChildIntoBoundingRegion(child, area);
			}
		}

		protected virtual void OnAppearing()
		{
		}

		protected virtual bool OnBackButtonPressed()
		{
			if (RealParent is BaseShellItem || RealParent is Shell)
				return false;


			var window = RealParent as Window;
			if (window == null || this == window.Page)
				return false;

			var canceled = false;
			EventHandler handler = (sender, args) => { canceled = true; };
			window.PopCanceled += handler;
			Navigation
				.PopModalAsync()
				.FireAndForget(Handler);

			window.PopCanceled -= handler;
			return !canceled;
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();
			foreach (ToolbarItem toolbarItem in ToolbarItems)
			{
				SetInheritedBindingContext(toolbarItem, BindingContext);
			}

			foreach (MenuBarItem menubarItem in MenuBarItems)
			{
				SetInheritedBindingContext(menubarItem, BindingContext);
			}

			if (_titleView != null)
				SetInheritedBindingContext(_titleView, BindingContext);
		}

		protected virtual void OnChildMeasureInvalidated(object sender, EventArgs e)
		{
			InvalidationTrigger trigger = (e as InvalidationEventArgs)?.Trigger ?? InvalidationTrigger.Undefined;
			OnChildMeasureInvalidated((VisualElement)sender, trigger);
		}

		protected virtual void OnDisappearing()
		{
		}

		protected override void OnParentSet()
		{
			if (!Application.IsApplicationOrWindowOrNull(RealParent) && !(RealParent is Page) && !(RealParent is BaseShellItem))
				throw new InvalidOperationException("Parent of a Page must also be a Page");
			base.OnParentSet();
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			_allocatedFlag = true;
			base.OnSizeAllocated(width, height);
			UpdateChildrenLayout();
		}

		protected void UpdateChildrenLayout()
		{
			if (!ShouldLayoutChildren())
				return;

			var logicalChildren = ((IElementController)this).LogicalChildren;
			var startingLayout = new List<Rect>(logicalChildren.Count);
			foreach (Element el in logicalChildren)
			{
				if (el is VisualElement c)
					startingLayout.Add(c.Bounds);
			}

			double x = Padding.Left;
			double y = Padding.Top;
			double w = Math.Max(0, Width - Padding.HorizontalThickness);
			double h = Math.Max(0, Height - Padding.VerticalThickness);

			LayoutChildren(x, y, w, h);

			for (var i = 0; i < logicalChildren.Count; i++)
			{
				var element = logicalChildren[i];
				if (element is VisualElement c)
				{
					if (startingLayout.Count <= i || c.Bounds != startingLayout[i])
					{
						LayoutChanged?.Invoke(this, EventArgs.Empty);
						return;
					}
				}
			}
		}

		internal virtual void OnChildMeasureInvalidated(VisualElement child, InvalidationTrigger trigger)
		{
			var container = this as IPageContainer<Page>;
			if (container != null)
			{
				Page page = container.CurrentPage;
				if (page != null && page.IsVisible && (!page.IsPlatformEnabled || !page.IsPlatformStateConsistent))
					return;
			}
			else
			{
				var logicalChildren = ((IElementController)this).LogicalChildren;
				for (var i = 0; i < logicalChildren.Count; i++)
				{
					var v = logicalChildren[i] as VisualElement;
					if (v != null && v.IsVisible && (!v.IsPlatformEnabled || !v.IsPlatformStateConsistent))
						return;
				}
			}

			_allocatedFlag = false;
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
			if (!_allocatedFlag && Width >= 0 && Height >= 0)
			{
				SizeAllocated(Width, Height);
			}
		}


		internal void OnAppearing(Action action)
		{
			if (_hasAppeared)
				action();
			else
			{
				EventHandler eventHandler = null;
				eventHandler = (_, __) =>
				{
					this.Appearing -= eventHandler;
					action();
				};

				this.Appearing += eventHandler;
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='SendAppearing']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendAppearing()
		{
			// Only fire appearing if the page has been added to the windows
			// Visual Hierarchy
			// The window/application will take care of re-firing this appearing 
			// if it needs to
			var window = this.FindParentOfType<IWindow>();
			if (window?.Parent == null)
			{
				return;
			}

			if (_hasAppeared)
				return;

			_hasAppeared = true;

			if (IsBusy)
			{
#pragma warning disable CS0618 // TODO: Remove when we internalize/replace MessagingCenter
				if (IsPlatformEnabled)
					MessagingCenter.Send(this, BusySetSignalName, true);
				else
					_pendingActions.Add(() => MessagingCenter.Send(this, BusySetSignalName, true));
			}
#pragma warning restore CS0618 // Type or member is obsolete

			OnAppearing();
			Appearing?.Invoke(this, EventArgs.Empty);

			var pageContainer = this as IPageContainer<Page>;
			pageContainer?.CurrentPage?.SendAppearing();

			FindApplication(this)?.OnPageAppearing(this);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Page.xml" path="//Member[@MemberName='SendDisappearing']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendDisappearing()
		{
			if (!_hasAppeared)
				return;

			_hasAppeared = false;

#pragma warning disable CS0618 // TODO: Remove when we internalize/replace MessagingCenter
			if (IsBusy)
				MessagingCenter.Send(this, BusySetSignalName, false);
#pragma warning restore CS0618 // Type or member is obsolete

			var pageContainer = this as IPageContainer<Page>;
			pageContainer?.CurrentPage?.SendDisappearing();

			OnDisappearing();
			Disappearing?.Invoke(this, EventArgs.Empty);

			FindApplication(this)?.OnPageDisappearing(this);
		}

		Application FindApplication(Element element)
		{
			if (element == null)
				return null;

			return (element.Parent is Application app) ? app : FindApplication(element.Parent);
		}

		void InternalChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				for (var i = 0; i < e.OldItems.Count; i++)
				{
					var item = (Element)e.OldItems[i];
					if (item is VisualElement visual)
						visual.MeasureInvalidated -= OnChildMeasureInvalidated;
					OnChildRemoved(item, e.OldStartingIndex + i);
				}
			}

			if (e.NewItems != null)
			{
				foreach (Element item in e.NewItems)
				{
					if (item is VisualElement visual)
						OnInternalAdded(visual);
					else
						OnChildAdded(item);
				}
			}
		}

		void OnInternalAdded(VisualElement view)
		{
			view.MeasureInvalidated += OnChildMeasureInvalidated;

			OnChildAdded(view);
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		void OnPageBusyChanged()
		{
			if (!_hasAppeared)
				return;
#pragma warning disable CS0618 // TODO: Remove when we internalize/replace MessagingCenter
			MessagingCenter.Send(this, BusySetSignalName, IsBusy);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		void OnToolbarItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (args.NewItems != null)
			{
				foreach (IElementDefinition item in args.NewItems)
					item.Parent = this;
			}

			if (args.OldItems != null)
			{
				foreach (IElementDefinition item in args.OldItems)
					item.Parent = null;
			}
		}

		bool ShouldLayoutChildren()
		{
			var logicalChildren = ((IElementController)this).LogicalChildren;
			if (logicalChildren.Count == 0 || Width <= 0 || Height <= 0 || !IsPlatformStateConsistent)
				return false;

			var container = this as IPageContainer<Page>;
			if (container?.CurrentPage != null)
			{
				if (InternalChildren.Contains(container.CurrentPage))
					return container.CurrentPage.IsPlatformEnabled && container.CurrentPage.IsPlatformStateConsistent;
				return true;
			}

			var any = false;
			for (var i = 0; i < logicalChildren.Count; i++)
			{
				var v = logicalChildren[i] as VisualElement;
				if (v != null && (!v.IsPlatformEnabled || !v.IsPlatformStateConsistent))
				{
					any = true;
					break;
				}
			}
			return !any;
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, Page> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		internal void SetTitleView(View oldTitleView, View newTitleView)
		{
			if (oldTitleView != null)
				oldTitleView.Parent = null;

			if (newTitleView != null)
				newTitleView.Parent = this;

			_titleView = newTitleView;
		}

		internal bool HasNavigatedTo { get; private set; }

		Paint IView.Background
		{
			get
			{
				if (!Brush.IsNullOrEmpty(Background))
					return Background;
				if (!ImageSource.IsNullOrEmpty(BackgroundImageSource))
					return new ImageSourcePaint(BackgroundImageSource);
				if (BackgroundColor.IsNotDefault())
					return new SolidColorBrush(BackgroundColor);

				return null;
			}
		}

		Toolbar _toolbar;
		IToolbar IToolbarElement.Toolbar
		{
			get => _toolbar;
		}

		internal Toolbar Toolbar
		{
			get => _toolbar;
			set
			{
				_toolbar = value;
				Handler?.UpdateValue(nameof(IToolbarElement.Toolbar));
			}
		}

		internal void SendNavigatedTo(NavigatedToEventArgs args)
		{
			HasNavigatedTo = true;
			NavigatedTo?.Invoke(this, args);
			OnNavigatedTo(args);
			(this as IPageContainer<Page>)?.CurrentPage?.SendNavigatedTo(args);
		}

		internal void SendNavigatingFrom(NavigatingFromEventArgs args)
		{
			NavigatingFrom?.Invoke(this, args);
			OnNavigatingFrom(args);
			(this as IPageContainer<Page>)?.CurrentPage?.SendNavigatingFrom(args);
		}

		internal void SendNavigatedFrom(NavigatedFromEventArgs args)
		{
			HasNavigatedTo = false;
			NavigatedFrom?.Invoke(this, args);
			OnNavigatedFrom(args);
			(this as IPageContainer<Page>)?.CurrentPage?.SendNavigatedFrom(args);
		}

		public event EventHandler<NavigatedToEventArgs> NavigatedTo;
		public event EventHandler<NavigatingFromEventArgs> NavigatingFrom;
		public event EventHandler<NavigatedFromEventArgs> NavigatedFrom;

		protected virtual void OnNavigatedTo(NavigatedToEventArgs args) { }
		protected virtual void OnNavigatingFrom(NavigatingFromEventArgs args) { }
		protected virtual void OnNavigatedFrom(NavigatedFromEventArgs args) { }

		public virtual Window GetParentWindow()
			=> this.FindParentOfType<Window>();
	}
}
