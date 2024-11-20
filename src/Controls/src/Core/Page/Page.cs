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
	/// <summary>
	/// A <see cref="VisualElement" /> that occupies the entire screen.
	/// </summary>
	/// <remarks><see cref = "Page" /> is primarily a base class for more useful derived types. Objects that are derived from the <see cref="Page"/> class are most prominently used as the top level UI element in .NET MAUI applications. In addition to their role as the main pages of applications, <see cref="Page"/> objects and their descendants can be used with navigation classes, such as <see cref="NavigationPage"/> or <see cref="FlyoutPage"/>, among others, to provide rich user experiences that conform to the expected behaviors on each platform.
	/// </remarks>
	public partial class Page : VisualElement, ILayout, IPageController, IElementConfiguration<Page>, IPaddingElement, ISafeAreaView, ISafeAreaView2, IView, ITitledElement, IToolbarElement
#if IOS
	,IiOSPageSpecifics
#endif
	{
		/// <summary>
		/// The identifier used by the internal messaging system to set <see cref="IsBusy"/>.
		/// </summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		public const string BusySetSignalName = "Microsoft.Maui.Controls.BusySet";

		/// <summary>
		/// The identifier used by the internal messaging system to display an alert dialog.
		/// </summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		public const string AlertSignalName = "Microsoft.Maui.Controls.SendAlert";

		/// <summary>
		/// The identifier used by the internal messaging system to display a prompt dialog.
		/// </summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		public const string PromptSignalName = "Microsoft.Maui.Controls.SendPrompt";

		/// <summary>
		/// The identifier used by the internal messaging system to display an action sheet.
		/// </summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		public const string ActionSheetSignalName = "Microsoft.Maui.Controls.ShowActionSheet";

		internal static readonly BindableProperty IgnoresContainerAreaProperty = BindableProperty.Create(nameof(IgnoresContainerArea), typeof(bool), typeof(Page), false);

		/// <summary>Bindable property for <see cref="BackgroundImageSource"/>.</summary>
		public static readonly BindableProperty BackgroundImageSourceProperty = BindableProperty.Create(nameof(BackgroundImageSource), typeof(ImageSource), typeof(Page), default(ImageSource));

		/// <summary>Bindable property for <see cref="IsBusy"/>.</summary>
		public static readonly BindableProperty IsBusyProperty = BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(Page), false, propertyChanged: (bo, o, n) => ((Page)bo).OnPageBusyChanged());

		/// <summary>Bindable property for <see cref="Padding"/>.</summary>
		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		/// <summary>Bindable property for <see cref="Title"/>.</summary>
		public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(Page), null);

		/// <summary>Bindable property for <see cref="IconImageSource"/>.</summary>
		public static readonly BindableProperty IconImageSourceProperty = BindableProperty.Create(nameof(IconImageSource), typeof(ImageSource), typeof(Page), default(ImageSource));

		readonly Lazy<PlatformConfigurationRegistry<Page>> _platformConfigurationRegistry;

		Rect _containerArea;

		bool _containerAreaSet;

		bool _hasAppeared;
		private protected bool HasAppeared => _hasAppeared;

		internal View TitleView;

		List<Action> _pendingActions = new List<Action>();

		/// <summary>
		/// Initializes a new instance of the <see cref="Page"/> class.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the <see cref="ImageSource"/> that will be used as the background for this page. This is a bindable property.
		/// </summary>
		public ImageSource BackgroundImageSource
		{
			get { return (ImageSource)GetValue(BackgroundImageSourceProperty); }
			set { SetValue(BackgroundImageSourceProperty, value); }
		}

		/// <summary>
		/// Gets or sets the <see cref="ImageSource"/> to be used for the icon associated to this page. This is a bindable property.
		/// </summary>
		/// <remarks>For example, this icon might be shown in the flyout menu or a tab bar together with <see cref="Title"/>.</remarks>
		public ImageSource IconImageSource
		{
			get { return (ImageSource)GetValue(IconImageSourceProperty); }
			set { SetValue(IconImageSourceProperty, value); }
		}

		/// <summary>
		/// Gets or sets the page busy state. This will cause the platform specific global activity indicator to show a busy state.
		/// This is a bindable property.
		/// </summary>
		/// <remarks>
		/// <para>Setting <see cref="IsBusy"/> to <see langword="true"/> on multiple pages at once will cause the global activity indicator to run until all are set back to <see langword="false"/>. It is the developer's responsibility to unset the <see cref="IsBusy"/> flag before cleaning up a page.</para>
		/// </remarks>
		public bool IsBusy
		{
			get { return (bool)GetValue(IsBusyProperty); }
			set { SetValue(IsBusyProperty, value); }
		}

		/// <summary>
		/// Gets or sets the space between the content of the page and its border. This is a bindable property.
		/// </summary>
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
			(this as IView).InvalidateMeasure();
		}

		/// <summary>
		/// Gets or sets the page's title.
		/// </summary>
		/// <remarks>For example, this title might be shown in the flyout menu or a tab bar together with <see cref="IconImageSource"/>.</remarks>
		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		/// <summary>
		/// Gets the <see cref="ToolbarItem"/> objects for this page, implemented in a platform-specific manner.
		/// </summary>
		public IList<ToolbarItem> ToolbarItems { get; internal set; }

		/// <summary>
		/// Gets the <see cref="MenuBarItem"/> objects for this page, implemented in a platform-specific manner.
		/// </summary>
		public IList<MenuBarItem> MenuBarItems { get; internal set; }

		/// <summary>
		/// Gets or sets the area this page is contained in.
		/// </summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
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

		/// <summary>
		/// Gets or sets a value that determines whether to ignore the <see cref="ContainerArea"/>. This is a bindable property.
		/// </summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IgnoresContainerArea
		{
			get { return (bool)GetValue(IgnoresContainerAreaProperty); }
			set { SetValue(IgnoresContainerAreaProperty, value); }
		}

		/// <summary>
		/// Gets the internal collection of child elements contained in this page.
		/// </summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ObservableCollection<Element> InternalChildren { get; } = new ObservableCollection<Element>();

		/// <inheritdoc/>
		bool ISafeAreaView.IgnoreSafeArea => !On<PlatformConfiguration.iOS>().UsingSafeArea();

#if IOS
		/// <inheritdoc/>
		bool IiOSPageSpecifics.IsHomeIndicatorAutoHidden
		{
			get
			{
				if (Parent is Page page && page.IsSet(PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty))
					return page.On<PlatformConfiguration.iOS>().PrefersHomeIndicatorAutoHidden();

				return On<PlatformConfiguration.iOS>().PrefersHomeIndicatorAutoHidden();
			}
		}

		/// <inheritdoc/>
		int IiOSPageSpecifics.PrefersStatusBarHiddenMode
		{
			get
			{
				if (Parent is Page page && page.IsSet(PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty))
					return (int)page.On<PlatformConfiguration.iOS>().PrefersStatusBarHidden();

				return (int)On<PlatformConfiguration.iOS>().PrefersStatusBarHidden();
			}
		}

		/// <inheritdoc/>
		int IiOSPageSpecifics.PreferredStatusBarUpdateAnimationMode
		{
			get
			{
				if (Parent is Page page && page.IsSet(PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty))
					return (int)page.On<PlatformConfiguration.iOS>().PreferredStatusBarUpdateAnimation();

				return (int)On<PlatformConfiguration.iOS>().PreferredStatusBarUpdateAnimation();
			}
		}
#endif

		/// <inheritdoc/>
		Thickness ISafeAreaView2.SafeAreaInsets
		{
			set
			{
				On<PlatformConfiguration.iOS>().SetSafeAreaInsets(value);
			}
		}

		/// <summary>
		/// Raised when the children of this page, and thus potentially the layout, have changed.
		/// </summary>
		public event EventHandler LayoutChanged;

		/// <summary>
		/// Raised when this page is visually appearing on screen.
		/// </summary>
		public event EventHandler Appearing;

		/// <summary>
		/// Raised when this page is visually disappearing from the screen.
		/// </summary>
		public event EventHandler Disappearing;

		/// <inheritdoc cref="DisplayActionSheet(string, string, string, FlowDirection, string[])"/>
		public Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons)
		{
			return DisplayActionSheet(title, cancel, destruction, FlowDirection.MatchParent, buttons);
		}

		/// <summary>
		/// Displays a platform action sheet, allowing the application user to choose from several buttons.
		/// </summary>
		/// <param name="title">Title of the displayed action sheet. Can be <see langword="null"/> to hide the title.</param>
		/// <param name="cancel">Text to be displayed in the 'Cancel' button. Can be null to hide the cancel action.</param>
		/// <param name="destruction">Text to be displayed in the 'Destruct' button. Can be <see langword="null"/> to hide the destructive option.</param>
		/// <param name="flowDirection">The flow direction to be used by the action sheet.</param>
		/// <param name="buttons">Text labels for additional buttons.</param>
		/// <returns>A <see cref="Task"/> that displays an action sheet and returns the string caption of the button pressed by the user.</returns>
		/// <remarks>Developers should be aware that Windows line endings, CR-LF, only work on Windows systems, and are incompatible with iOS and Android. A particular consequence of this is that characters that appear after a CR-LF, (For example, in the title) may not be displayed on non-Windows platforms. Developers must use the correct line endings for each of the targeted systems.</remarks>
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

		/// <inheritdoc cref="DisplayAlert(string, string, string, string, FlowDirection)"/>
		public Task DisplayAlert(string title, string message, string cancel)
		{
			return DisplayAlert(title, message, null, cancel, FlowDirection.MatchParent);
		}

		/// <inheritdoc cref="DisplayAlert(string, string, string, string, FlowDirection)"/>
		public Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
		{
			return DisplayAlert(title, message, accept, cancel, FlowDirection.MatchParent);
		}

		/// <inheritdoc cref="DisplayAlert(string, string, string, string, FlowDirection)"/>
		public Task DisplayAlert(string title, string message, string cancel, FlowDirection flowDirection)
		{
			return DisplayAlert(title, message, null, cancel, flowDirection);
		}

		/// <summary>
		/// Displays an alert dialog to the application user with a single cancel button.
		/// </summary>
		/// <param name="title">The title of the alert dialog. Can be <see langword="null"/> to hide the title.</param>
		/// <param name="message">The body text of the alert dialog.</param>
		/// <param name="accept">Text to be displayed on the 'Accept' button. Can be <see langword="null"/> to hide this button.</param>
		/// <param name="cancel">Text to be displayed on the 'Cancel' button.</param>
		/// <param name="flowDirection">The flow direction to be used by the alert.</param>
		/// <returns>A <see cref="Task"/> that contains the user's choice as a <see cref="bool"/> value. <see langword="true"/> indicates that the user accepted the alert. <see langword="false"/> indicates that the user cancelled the alert.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="cancel"/> is <see langword="null"/> or empty.</exception>
		public Task<bool> DisplayAlert(string title, string message, string accept, string cancel, FlowDirection flowDirection)
		{
			if (string.IsNullOrEmpty(cancel))
				throw new ArgumentNullException(nameof(cancel));

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

		/// <summary>
		/// Displays a prompt dialog to the application user with the intent to capture a single string value.
		/// </summary>
		/// <param name="title">The title of the prompt dialog.</param>
		/// <param name="message">The body text of the prompt dialog.</param>
		/// <param name="accept">Text to be displayed on the 'Accept' button.</param>
		/// <param name="cancel">Text to be displayed on the 'Cancel' button.</param>
		/// <param name="placeholder">The placeholder text to display in the prompt. Can be <see langword="null"/> when no placeholder is desired.</param>
		/// <param name="maxLength">The maximum length of the user response.</param>
		/// <param name="keyboard">The keyboard type to use for the user response.</param>
		/// <param name="initialValue">A pre-defined response that will be displayed, and which can be edited by the user.</param>
		/// <returns>A <see cref="Task"/> that displays a prompt display and returns the string value as entered by the user.</returns>
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

		/// <summary>
		/// Forces the page to perform a layout pass.
		/// </summary>
		public void ForceLayout()
		{
			SizeAllocated(Width, Height);
		}

		/// <summary>
		/// Calls <see cref="OnBackButtonPressed"/>.
		/// </summary>
		/// <returns><see langword="true"/> when the back navigation was handled by the <see cref="OnBackButtonPressed"/> override, otherwise <see langword="false"/>.</returns>
		public bool SendBackButtonPressed()
		{
			return OnBackButtonPressed();
		}

		/// <summary>
		/// Lays out the child elements when the layout is invalidated.
		/// </summary>
		/// <param name="x">X-coordinate of the top left corner of the bounding rectangle.</param>
		/// <param name="y">Y-coordinate of the top left corner of the bounding rectangle.</param>
		/// <param name="width">Width of the bounding rectangle.</param>
		/// <param name="height">Height of the bounding rectangle.</param>
		[Obsolete("Use ArrangeOverride instead")]
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

			IList<Element> elements = this.InternalChildren;
			foreach (Element element in elements)
			{
				var child = element as VisualElement;
				if (child == null)
					continue;

				var page = child as Page;
#pragma warning disable CS0618 // Type or member is obsolete
				if (page != null && page.IgnoresContainerArea)
					Maui.Controls.Compatibility.Layout.LayoutChildIntoBoundingRegion(child, originalArea);
				else
					Maui.Controls.Compatibility.Layout.LayoutChildIntoBoundingRegion(child, area);
#pragma warning restore CS0618 // Type or member is obsolete
			}
		}

		/// <summary>
		/// When overridden in a derived class, allows application developers to customize behavior immediately prior to the page becoming visible.
		/// </summary>
		protected virtual void OnAppearing()
		{
		}

		/// <summary>
		/// Determines the behavior when the back button of the page is pressed.
		/// </summary>
		/// <returns><see langword="true"/> when the back navigation was handled by the override, otherwise <see langword="false"/>.</returns>
		/// <remarks>
		/// <para>Application developers can override this method to provide behavior when the back button is pressed. 
		/// When overridden to handle or cancel the navigation yourself, this method should return <see langword="true"/>.</para>
		/// <para>This only works on Android and UWP for the hardware back button. On iOS, this method will never be called because there is no hardware back button.</para>
		/// </remarks>
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

		/// <summary>
		/// Invoked whenever the binding context of the page changes. Override this method to add class handling for this event.
		/// </summary>
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

			if (TitleView != null)
				SetInheritedBindingContext(TitleView, BindingContext);
		}


		internal override void OnChildMeasureInvalidatedInternal(VisualElement child, InvalidationTrigger trigger, int depth)
		{
			// TODO: once we remove old Xamarin public signatures we can invoke `OnChildMeasureInvalidated(VisualElement, InvalidationTrigger)` directly
			OnChildMeasureInvalidated(child, new InvalidationEventArgs(trigger, depth));
		}

		/// <summary>
		/// Indicates that the preferred size of a child <see cref="Element"/> has changed.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">The event arguments.</param>
		protected virtual void OnChildMeasureInvalidated(object sender, EventArgs e)
		{
			var depth = 0;
			InvalidationTrigger trigger;
			if (e is InvalidationEventArgs args)
			{
				trigger = args.Trigger;
				depth = args.CurrentInvalidationDepth;
			}
			else
			{
				trigger = InvalidationTrigger.Undefined;
			}

			OnChildMeasureInvalidated((VisualElement)sender, trigger, depth);
		}

		/// <summary>
		/// When overridden in a derived class, allows the application developer to customize behavior as the page disappears.
		/// </summary>
		/// <remarks>
		/// This method is called when the page disappears due to navigating away from the page within the app. It is <b>not</b> called when the app disappears due to an event external to the app (e.g. user navigates to the home screen or another app, a phone call is received, the device is locked, the device is turned off).
		/// </remarks>
		protected virtual void OnDisappearing()
		{
		}

		/// <summary>
		/// Called when the page's <see cref="Element.Parent"/> property has changed.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when the page's <see cref="Element.RealParent"/> can't be casted to <see cref="Page"/> or <see cref="BaseShellItem"/>.</exception>
		protected override void OnParentSet()
		{
			if (!Application.IsApplicationOrWindowOrNull(RealParent) && !(RealParent is Page) && !(RealParent is BaseShellItem))
				throw new InvalidOperationException("Parent of a Page must also be a Page");
			base.OnParentSet();
		}

		/// <summary>
		/// Indicates that the page has been assigned a size.
		/// </summary>
		/// <param name="width">The width allocated to the page.</param>
		/// <param name="height">The height allocated to the page.</param>
		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);

			if (Handler is null)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				UpdateChildrenLayout();
#pragma warning restore CS0618 // Type or member is obsolete
			}
		}

		/// <summary>
		/// Requests that the child <see cref="Element"/>s of the page update their layouts.
		/// </summary>
		[Obsolete("Use ArrangeOverride instead")]
		protected void UpdateChildrenLayout()
		{
			if (!ShouldLayoutChildren())
				return;

			var logicalChildren = this.InternalChildren;
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

		internal virtual void OnChildMeasureInvalidated(VisualElement child, InvalidationTrigger trigger, int depth)
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
				var logicalChildren = this.InternalChildren;
				for (var i = 0; i < logicalChildren.Count; i++)
				{
					var v = logicalChildren[i] as VisualElement;
					if (v != null && v.IsVisible && (!v.IsPlatformEnabled || !v.IsPlatformStateConsistent))
						return;
				}
			}

			if (depth <= 1)
			{
				InvalidateMeasureInternal(new InvalidationEventArgs(InvalidationTrigger.MeasureChanged, depth));
			}
			else
			{
				FireMeasureChanged(trigger, depth);
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

		/// <summary>
		/// Sends the signal to the page that it is about to visually appear on screen.
		/// </summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
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

		/// <summary>
		/// Sends the signal to the page that it is about to be visually hidden from the screen.
		/// </summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
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
					RemoveLogicalChild(item);
				}
			}

			if (e.NewItems != null)
			{
				int index = e.NewStartingIndex;

				foreach (Element item in e.NewItems)
				{
					int insertIndex = index;
					if (insertIndex < 0)
					{
						insertIndex = InternalChildren.IndexOf(item);
					}

					InsertLogicalChild(insertIndex, item);

					if (item is VisualElement)
					{
						InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
					}

					if (index >= 0)
					{
						index++;
					}
				}
			}
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
			var logicalChildren = this.InternalChildren;
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
			TitleView = newTitleView;
		}

		internal bool HasNavigatedTo { get; private set; }

		/// <inheritdoc/>
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
			set => ToolbarElement.SetValue(ref _toolbar, value, Handler);
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

		internal void SendNavigatedFrom(NavigatedFromEventArgs args, bool disconnectHandlers = true)
		{
			HasNavigatedTo = false;
			NavigatedFrom?.Invoke(this, args);
			OnNavigatedFrom(args);
			(this as IPageContainer<Page>)?.CurrentPage?.SendNavigatedFrom(args, false);

			if (!disconnectHandlers)
			{
				return;
			}

			if (args.NavigationType == NavigationType.Pop ||
				args.NavigationType == NavigationType.PopToRoot)
			{
				if (!this.IsLoaded)
				{
					this.DisconnectHandlers();
				}
				else
				{
					this.OnUnloaded(() => this.DisconnectHandlers());
				}
			}
		}

		/// <summary>
		/// Raised after the page was navigated to.
		/// </summary>
		public event EventHandler<NavigatedToEventArgs> NavigatedTo;

		/// <summary>
		/// Raised before navigating away from the page.
		/// </summary>
		public event EventHandler<NavigatingFromEventArgs> NavigatingFrom;

		/// <summary>
		/// Raised after the page was navigated away from.
		/// </summary>
		public event EventHandler<NavigatedFromEventArgs> NavigatedFrom;

		/// <summary>
		/// When overridden in a derived class, allows application developers to customize behavior immediately after the page was navigated to.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void OnNavigatedTo(NavigatedToEventArgs args) { }

		/// <summary>
		/// When overridden in a derived class, allows application developers to customize behavior immediately prior to the page being navigated away from.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void OnNavigatingFrom(NavigatingFromEventArgs args) { }

		/// <summary>
		/// When overridden in a derived class, allows application developers to customize behavior immediately after the page was being navigated away from.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		protected virtual void OnNavigatedFrom(NavigatedFromEventArgs args) { }

		/// <summary>
		/// Retrieves the parent window that contains the page.
		/// </summary>
		/// <returns>The <see cref="Window"/> instance that parents the page.</returns>
		public virtual Window GetParentWindow()
			=> this.FindParentOfType<Window>();
	}
}
