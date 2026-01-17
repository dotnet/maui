#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui;
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
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	public partial class Page : VisualElement, ILayout, IPageController, IElementConfiguration<Page>, IPaddingElement, ISafeAreaView2, IView, ITitledElement, IToolbarElement, ISafeAreaView, IConstrainedView
#if IOS
	, IiOSPageSpecifics
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
		[Obsolete("Page.IsBusy has been deprecated and will be removed in .NET 11")]
		public static readonly BindableProperty IsBusyProperty = BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(Page), false, propertyChanged: (bo, o, n) => ((Page)bo).OnPageBusyChanged());

		/// <summary>Bindable property for <see cref="Padding"/>.</summary>
		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		/// <summary>Bindable property for <see cref="Title"/>.</summary>
		public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(Page), null);

		/// <summary>Bindable property for <see cref="IconImageSource"/>.</summary>
		public static readonly BindableProperty IconImageSourceProperty = BindableProperty.Create(nameof(IconImageSource), typeof(ImageSource), typeof(Page), default(ImageSource), propertyChanged: OnImageSourceChanged);

		readonly Lazy<PlatformConfigurationRegistry<Page>> _platformConfigurationRegistry;

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
		[Obsolete("Page.IsBusy has been deprecated and will be removed in .NET 11")]
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

		[Obsolete("This property is obsolete and will be removed in a future version. Use Bounds instead.")]
		public Rect ContainerArea
		{
			get { return Bounds; }
			set
			{
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
		// TODO: Mark this obsolete and fix everywhere that references this property to use the more correct add/remove logical children
		public ObservableCollection<Element> InternalChildren { get; } = new ObservableCollection<Element>();

		/// <inheritdoc/>
#pragma warning disable CS0618 // Type or member is obsolete
		bool ISafeAreaView.IgnoreSafeArea => !On<PlatformConfiguration.iOS>().UsingSafeArea();
#pragma warning restore CS0618 // Type or member is obsolete

		bool IConstrainedView.HasFixedConstraints => true;

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

		/// <inheritdoc cref="ISafeAreaView2.GetSafeAreaRegionsForEdge"/>
		SafeAreaRegions ISafeAreaView2.GetSafeAreaRegionsForEdge(int edge)
		{
			var ignoreSafeArea = ((ISafeAreaView)this).IgnoreSafeArea;
			if (ignoreSafeArea)
			{
				return SafeAreaRegions.None; // If legacy says "ignore", return None (edge-to-edge)
			}
			else
			{
				return SafeAreaRegions.Container; // If legacy says "don't ignore", return Container
			}
		}

		/// <summary>
		/// Raised when the children of this page, and thus potentially the layout, have changed.
		/// </summary>
		[Obsolete("Use SizeChanged.")]
#pragma warning disable CS0067 // Type or member is obsolete
		public event EventHandler LayoutChanged;
#pragma warning disable CS0067 // Type or member is obsolete

		/// <summary>
		/// Raised when this page is visually appearing on screen.
		/// </summary>
		public event EventHandler Appearing;

		/// <summary>
		/// Raised when this page is visually disappearing from the screen.
		/// </summary>
		public event EventHandler Disappearing;

		/// <inheritdoc cref="DisplayActionSheetAsync(string, string, string, FlowDirection, string[])"/>
		[Obsolete("Use DisplayActionSheetAsync instead")]
		public Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons)
			=> DisplayActionSheetAsync(title, cancel, destruction, FlowDirection.MatchParent, buttons);

		/// <inheritdoc cref="DisplayActionSheetAsync(string, string, string, FlowDirection, string[])"/>
		[Obsolete("Use DisplayActionSheetAsync instead")]
		public Task<string> DisplayActionSheet(string title, string cancel, string destruction, FlowDirection flowDirection, params string[] buttons)
			=> DisplayActionSheetAsync(title, cancel, destruction, flowDirection, buttons);

		/// <inheritdoc cref="DisplayActionSheetAsync(string, string, string, FlowDirection, string[])"/>
		public Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, params string[] buttons)
			=> DisplayActionSheetAsync(title, cancel, destruction, FlowDirection.MatchParent, buttons);

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
		public Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, FlowDirection flowDirection, params string[] buttons)
		{
			var args = new ActionSheetArguments(title, cancel, destruction, buttons);

			args.FlowDirection = flowDirection;
			if (IsPlatformEnabled)
				Window.AlertManager.RequestActionSheet(this, args);
			else
				_pendingActions.Add(() => Window.AlertManager.RequestActionSheet(this, args));

			return args.Result.Task;
		}

		/// <inheritdoc cref="DisplayAlertAsync(string, string, string, string, FlowDirection)"/>
		[Obsolete("Use DisplayAlertAsync instead")]
		public Task DisplayAlert(string title, string message, string cancel)

			=> DisplayAlertAsync(title, message, null, cancel, FlowDirection.MatchParent);

		/// <inheritdoc cref="DisplayAlertAsync(string, string, string, string, FlowDirection)"/>
		[Obsolete("Use DisplayAlertAsync instead")]
		public Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
			=> DisplayAlertAsync(title, message, accept, cancel, FlowDirection.MatchParent);

		/// <inheritdoc cref="DisplayAlertAsync(string, string, string, string, FlowDirection)"/>
		[Obsolete("Use DisplayAlertAsync instead")]
		public Task DisplayAlert(string title, string message, string cancel, FlowDirection flowDirection)
			=> DisplayAlertAsync(title, message, null, cancel, flowDirection);

		/// <inheritdoc cref="DisplayAlertAsync(string, string, string, string, FlowDirection)"/>
		[Obsolete("Use DisplayAlertAsync instead")]
		public Task<bool> DisplayAlert(string title, string message, string accept, string cancel, FlowDirection flowDirection)
			=> DisplayAlertAsync(title, message, accept, cancel, flowDirection);

		/// <inheritdoc cref="DisplayAlertAsync(string, string, string, string, FlowDirection)"/>
		public Task DisplayAlertAsync(string title, string message, string cancel)
			=> DisplayAlertAsync(title, message, null, cancel, FlowDirection.MatchParent);

		/// <inheritdoc cref="DisplayAlertAsync(string, string, string, string, FlowDirection)"/>
		public Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel)
			=> DisplayAlertAsync(title, message, accept, cancel, FlowDirection.MatchParent);

		/// <inheritdoc cref="DisplayAlertAsync(string, string, string, string, FlowDirection)"/>
		public Task DisplayAlertAsync(string title, string message, string cancel, FlowDirection flowDirection)
			=> DisplayAlertAsync(title, message, null, cancel, flowDirection);

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
		public Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel, FlowDirection flowDirection)
		{
			if (string.IsNullOrEmpty(cancel))
			{
				throw new ArgumentNullException(nameof(cancel));
			}

			var args = new AlertArguments(title, message, accept, cancel);
			args.FlowDirection = flowDirection;

			if (IsPlatformEnabled)
				Window.AlertManager.RequestAlert(this, args);
			else
				_pendingActions.Add(() => Window.AlertManager.RequestAlert(this, args));

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

			if (IsPlatformEnabled)
				Window.AlertManager.RequestPrompt(this, args);
			else
				_pendingActions.Add(() => Window.AlertManager.RequestPrompt(this, args));

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


		internal override void OnChildMeasureInvalidated(VisualElement child, InvalidationTrigger trigger)
		{
			OnChildMeasureInvalidated(child, new InvalidationEventArgs(trigger));
			var propagatedTrigger = GetPropagatedTrigger(trigger);
			InvokeMeasureInvalidated(propagatedTrigger);
		}

		/// <summary>
		/// Indicates that the preferred size of a child <see cref="Element"/> has changed.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">The event arguments.</param>
		protected virtual void OnChildMeasureInvalidated(object sender, EventArgs e)
		{
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

#pragma warning disable CS0618 // TODO: Remove this API in .NET 11. Issue Link: https://github.com/dotnet/maui/issues/30155
			if (IsBusy)
			{
				if (IsPlatformEnabled)
					Window.AlertManager.RequestPageBusy(this, true);
				else
					_pendingActions.Add(() => Window.AlertManager.RequestPageBusy(this, true));
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

#pragma warning disable CS0618 // TODO: Remove this API in .NET 11. Issue Link: https://github.com/dotnet/maui/issues/30155
			if (IsBusy)
				Window.AlertManager.RequestPageBusy(this, false);
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
#pragma warning disable CS0618 // TODO: Remove this API in .NET 11. Issue Link: https://github.com/dotnet/maui/issues/30155
			Window.AlertManager.RequestPageBusy(this, IsBusy);
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
		
		// Only propagate to current page if it hasn't already received NavigatedTo
		// This prevents duplicate events when multiple navigation systems are involved
		var currentPage = (this as IPageContainer<Page>)?.CurrentPage;
		if (currentPage != null && !currentPage.HasNavigatedTo)
		{
			currentPage.SendNavigatedTo(args);
		}
	}		internal void SendNavigatingFrom(NavigatingFromEventArgs args)
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

		static void OnImageSourceChanged(BindableObject bindable, object oldvalue, object newValue)
		{
			if (oldvalue is ImageSource oldImageSource)
				oldImageSource.SourceChanged -= ((Page)bindable).OnImageSourceSourceChanged;

			if (newValue is ImageSource newImageSource)
				newImageSource.SourceChanged += ((Page)bindable).OnImageSourceSourceChanged;
		}

		void OnImageSourceSourceChanged(object sender, EventArgs e)
		{
			OnPropertyChanged(IconImageSourceProperty.PropertyName);
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

		private protected override string GetDebuggerDisplay()
		{
			var debugText = DebuggerDisplayHelpers.GetDebugText(nameof(BindingContext), BindingContext, nameof(Title), Title);
			return $"{this.GetType().FullName}: {debugText}";
		}
#nullable enable
		IDisposable? _navigationEventSubscription = null;

		/// <summary>
		/// Configures this page as the outgoing page during navigation, handling NavigatingFrom and NavigatedFrom events.
		/// </summary>
		/// <param name="incomingPage">The page being navigated to.</param>
		/// <param name="navigationType">The type of navigation being performed.</param>
		internal void ConfigureAsOutgoingPage(Page? incomingPage, NavigationType navigationType)
		{
			// Clean up any existing navigation event subscriptions to prevent duplicates
			CleanupNavigationEventSubscription();

			// Only fire NavigatingFrom if we previously fired NavigatedTo (to maintain event pairing)
			if (HasNavigatedTo)
			{
				SendNavigatingFrom(new NavigatingFromEventArgs(incomingPage , navigationType));
			}

			if (IsLoaded)
			{
				// Schedule NavigatedFrom to fire when page unloads
				_navigationEventSubscription = this.OnUnloaded(() =>
				{
					FireNavigatedFromAndCleanup(navigationType);
				});
			}
			else
			{
				// Page is not loaded, fire NavigatedFrom immediately
				FireNavigatedFromAndCleanup(navigationType);
			}
		}

		/// <summary>
		/// Configures this page as the incoming page during navigation, handling NavigatedTo events.
		/// </summary>
		/// <param name="outgoingPage">The page being navigated from.</param>
		internal void ConfigureAsIncomingPage(Page? outgoingPage)
		{
			// Configure the outgoing page first (preventing potential duplicate event wiring)
			outgoingPage?.ConfigureAsOutgoingPage(this, NavigationType.Replace);

			// Clean up any existing navigation event subscriptions to prevent duplicates
			CleanupNavigationEventSubscription();

			if (!IsLoaded)
			{
				// Page is not yet loaded, wait for Loaded event before firing NavigatedTo
				SetupLoadedEventHandling(outgoingPage);
			}
			else
			{
				// Page is already loaded, fire NavigatedTo immediately if not already fired
				if (!HasNavigatedTo)
				{
					SendNavigatedTo(new NavigatedToEventArgs(outgoingPage, NavigationType.Replace));
				}

				// Set up handler for future unloaded events
				SetupUnloadedEventHandling();
			}
		}

		/// <summary>
		/// Sets up event handling for when the page gets loaded.
		/// </summary>
		/// <param name="previousPage">The page that was navigated from.</param>
		private void SetupLoadedEventHandling(Page? previousPage)
		{
			EventHandler? onPageLoaded = null;
			onPageLoaded = (object? sender, EventArgs args) =>
			{
				if (sender is Page loadedPage && !loadedPage.HasNavigatedTo)
				{
					loadedPage.SendNavigatedTo(new NavigatedToEventArgs(previousPage, NavigationType.Replace));

					// Remove the loaded event handler to prevent duplicate firing
					if (onPageLoaded != null)
					{
						Loaded -= onPageLoaded;
					}

					// Set up handler for future unloaded events
					SetupUnloadedEventHandling();
				}
			};

			Loaded += onPageLoaded;

			// Store subscription for cleanup
			_navigationEventSubscription = new ActionDisposable(() =>
			{
				if (onPageLoaded != null)
				{
					Loaded -= onPageLoaded;
				}
			});
		}

		/// <summary>
		/// Sets up event handling for when the page gets unloaded.
		/// </summary>
		private void SetupUnloadedEventHandling()
		{
			EventHandler? onPageUnloaded = null;
			onPageUnloaded = (object? sender, EventArgs args) =>
			{
				if (sender is Page unloadedPage && unloadedPage.HasNavigatedTo)
				{
					// Fire navigation events when page unloads (ensure proper event pairing)
					unloadedPage.SendNavigatedFrom(new NavigatedFromEventArgs(unloadedPage, NavigationType.Replace));
				}

				// Remove the unloaded event handler to prevent duplicate firing
				if (onPageUnloaded != null)
				{
					Unloaded -= onPageUnloaded;
				}
			};

			Unloaded += onPageUnloaded;

			// Store subscription for cleanup
			_navigationEventSubscription = new ActionDisposable(() =>
			{
				if (onPageUnloaded != null)
				{
					Unloaded -= onPageUnloaded;
				}
			});
		}

		/// <summary>
		/// Fires the NavigatedFrom event and performs cleanup operations.
		/// </summary>
		/// <param name="navigationType">The type of navigation that occurred.</param>
		private void FireNavigatedFromAndCleanup(NavigationType navigationType)
		{
			if (HasNavigatedTo)
			{
				SendNavigatedFrom(new NavigatedFromEventArgs(this, navigationType));
			}

			this.DisconnectHandlers();
			CleanupNavigationEventSubscription();
		}

		/// <summary>
		/// Cleans up navigation event subscriptions to prevent memory leaks.
		/// </summary>
		private void CleanupNavigationEventSubscription()
		{
			_navigationEventSubscription?.Dispose();
			_navigationEventSubscription = null;
		}
	}
}