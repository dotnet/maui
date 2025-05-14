#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;

using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="Page"/> that manages two panes of information: a flyout that presents a menu or navigation, and a detail that presents the selected content.</summary>
	[ContentProperty(nameof(Detail))]
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
#if IOS || MACCATALYST
	[ElementHandler<Handlers.Compatibility.PhoneFlyoutPageRenderer>]
#elif WINDOWS || ANDROID || TIZEN
	[ElementHandler<FlyoutViewHandler>]
#endif
	public partial class FlyoutPage : Page, IFlyoutPageController, IElementConfiguration<FlyoutPage>, IFlyoutView
	{
		/// <summary>Bindable property for <see cref="IsGestureEnabled"/>.</summary>
		public static readonly BindableProperty IsGestureEnabledProperty = BindableProperty.Create(nameof(IsGestureEnabled), typeof(bool), typeof(FlyoutPage), true);

		/// <summary>Bindable property for <see cref="IsPresented"/>.</summary>
		public static readonly BindableProperty IsPresentedProperty = BindableProperty.Create(nameof(IsPresented), typeof(bool), typeof(FlyoutPage), default(bool),
			propertyChanged: OnIsPresentedPropertyChanged, propertyChanging: OnIsPresentedPropertyChanging, defaultValueCreator: GetDefaultValue);

		/// <summary>Bindable property for <see cref="FlyoutLayoutBehavior"/>.</summary>
		public static readonly BindableProperty FlyoutLayoutBehaviorProperty = BindableProperty.Create(nameof(FlyoutLayoutBehavior), typeof(FlyoutLayoutBehavior), typeof(FlyoutPage), default(FlyoutLayoutBehavior),
			propertyChanged: OnFlyoutLayoutBehaviorPropertyChanged);

		Page _detail;

		Rect _detailBounds;

		Page _flyout;

		Rect _flyoutBounds;

		IFlyoutPageController FlyoutPageController => this;

		/// <summary>Gets or sets the detail page that is used to display details about items on the flyout page.</summary>
		public Page Detail
		{
			get { return _detail; }
			set
			{
				if (_detail != null && value == null)
					throw new ArgumentNullException(nameof(value), "Detail cannot be set to null once a value is set.");

				if (_detail == value)
					return;

				if (value.RealParent != null)
					throw new InvalidOperationException("Detail must not already have a parent.");

				var previousDetail = _detail;

				previousDetail?.SendNavigatingFrom(new NavigatingFromEventArgs(destinationPage: value, navigationType: NavigationType.Replace));

				// Update the detail property
				OnPropertyChanging();
				if (_detail is not null)
					InternalChildren.Remove(_detail);
				_detail = value;
				InternalChildren.Add(_detail);
				OnPropertyChanged();

				// Handle Appearing/Disappearing events if the FlyoutPage has appeared
				if (HasAppeared)
				{
					previousDetail?.SendDisappearing();
					_detail?.SendAppearing();
				}

				// Send NavigatedFrom and NavigatedTo events
				if (previousDetail is not null)
				{
					previousDetail.SendNavigatedFrom(
						new NavigatedFromEventArgs(destinationPage: value, NavigationType.Replace));
				}

				_detail.SendNavigatedTo(new NavigatedToEventArgs(previousDetail, NavigationType.Replace));
			}
		}

		/// <summary>Gets or sets a value that indicates whether swipe gestures can open the flyout. This is a bindable property.</summary>
		public bool IsGestureEnabled
		{
			get { return (bool)GetValue(IsGestureEnabledProperty); }
			set { SetValue(IsGestureEnabledProperty, value); }
		}

		/// <summary>Gets or sets a value that indicates whether the flyout is presented. This is a bindable property.</summary>
		public bool IsPresented
		{
			get { return (bool)GetValue(IsPresentedProperty); }
			set { SetValue(IsPresentedProperty, value); }
		}

		/// <summary>Gets or sets the flyout page that is used to present a menu or navigation options.</summary>
		public Page Flyout
		{
			get { return _flyout; }
			set
			{
				if (_flyout != null && value == null)
					throw new ArgumentNullException(nameof(value), "Flyout cannot be set to null once a value is set");

				if (string.IsNullOrEmpty(value.Title))
					throw new InvalidOperationException("Title property must be set on Flyout page");

				if (_flyout == value)
					return;

				if (value.RealParent != null)
					throw new InvalidOperationException("Flyout must not already have a parent.");

				// TODO MAUI refine this to fire earlier
				var previousFlyout = _flyout;
				
				// TODO MAUI refine this to fire earlier
				previousFlyout?.SendNavigatingFrom(new NavigatingFromEventArgs(value, NavigationType.Replace));

				OnPropertyChanging();
				if (_flyout != null)
					InternalChildren.Remove(_flyout);
				_flyout = value;
				InternalChildren.Add(_flyout);
				OnPropertyChanged();

				if (this.HasAppeared)
				{
					previousFlyout?.SendDisappearing();
					_flyout?.SendAppearing();
				}
				
				previousFlyout?.SendNavigatedFrom(new NavigatedFromEventArgs(_flyout, NavigationType.Replace));
				_flyout?.SendNavigatedTo(new NavigatedToEventArgs(previousFlyout, NavigationType.Replace));
			}
		}

		/// <summary>Gets or sets a value that indicates how the flyout page displays on the screen. This is a bindable property.</summary>
		public FlyoutLayoutBehavior FlyoutLayoutBehavior
		{
			get { return (FlyoutLayoutBehavior)GetValue(FlyoutLayoutBehaviorProperty); }
			set { SetValue(FlyoutLayoutBehaviorProperty, value); }
		}

		bool IFlyoutPageController.CanChangeIsPresented { get; set; } = true;

		Rect IFlyoutPageController.DetailBounds
		{
			get { return _detailBounds; }
			set
			{
				_detailBounds = value;
				if (_detail == null)
					throw new InvalidOperationException("Detail must be set before using a FlyoutPage");
			}
		}

		Rect IFlyoutPageController.FlyoutBounds
		{
			get { return _flyoutBounds; }
			set
			{
				_flyoutBounds = value;
				if (_flyout == null)
					throw new InvalidOperationException("Flyout must be set before using a FlyoutPage");
			}
		}

		bool IFlyoutPageController.ShouldShowSplitMode
		{
			get
			{
				if (DeviceInfo.Idiom == DeviceIdiom.Phone)
					return false;

				FlyoutLayoutBehavior behavior = FlyoutLayoutBehavior;
				var orientation = Window.GetOrientation();

				bool isSplitOnLandscape = (behavior == FlyoutLayoutBehavior.SplitOnLandscape || behavior == FlyoutLayoutBehavior.Default) && orientation.IsLandscape();
				bool isSplitOnPortrait = behavior == FlyoutLayoutBehavior.SplitOnPortrait && orientation.IsPortrait();
				return behavior == FlyoutLayoutBehavior.Split || isSplitOnLandscape || isSplitOnPortrait;
			}
		}

		public event EventHandler IsPresentedChanged;

		/// <summary>Returns a value that indicates whether the toolbar should display a button to toggle the flyout.</summary>
		/// <returns><see langword="true"/> if the toolbar button should be shown; otherwise, <see langword="false"/>.</returns>
		public virtual bool ShouldShowToolbarButton()
		{
			if (DeviceInfo.Idiom == DeviceIdiom.Phone)
				return true;

			FlyoutLayoutBehavior behavior = FlyoutLayoutBehavior;
			var orientation = DeviceDisplay.MainDisplayInfo.Orientation;

			bool isSplitOnLandscape = (behavior == FlyoutLayoutBehavior.SplitOnLandscape || behavior == FlyoutLayoutBehavior.Default) && orientation.IsLandscape();
			bool isSplitOnPortrait = behavior == FlyoutLayoutBehavior.SplitOnPortrait && orientation.IsPortrait();
			return behavior != FlyoutLayoutBehavior.Split && !isSplitOnLandscape && !isSplitOnPortrait;
		}

		[Obsolete("Use ArrangeOverride instead")]
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			if (Flyout == null || Detail == null)
				throw new InvalidOperationException("Flyout and Detail must be set before using a FlyoutPage");
		}

		protected override void OnAppearing()
		{
			Flyout?.SendAppearing();
			Detail?.SendAppearing();

			FlyoutPageController.CanChangeIsPresented = true;
			UpdateFlyoutLayoutBehavior(this);
			base.OnAppearing();
		}

		protected override void OnDisappearing()
		{
			Flyout?.SendDisappearing();
			Detail?.SendDisappearing();

			base.OnDisappearing();
		}

		protected override bool OnBackButtonPressed()
		{
			if (IsPresented)
			{
				if (Flyout.SendBackButtonPressed())
					return true;
			}

			EventHandler<BackButtonPressedEventArgs> handler = BackButtonPressed;
			if (handler != null)
			{
				var args = new BackButtonPressedEventArgs();
				handler(this, args);
				if (args.Handled)
					return true;
			}

			if (Detail.SendBackButtonPressed())
			{
				return true;
			}

			return base.OnBackButtonPressed();
		}

		protected override void OnParentSet()
		{
			if (RealParent != null && (Flyout == null || Detail == null))
				throw new InvalidOperationException("Flyout and Detail must be set before adding FlyoutPage to a container");
			base.OnParentSet();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<BackButtonPressedEventArgs> BackButtonPressed;

		/// <summary>Updates the layout behavior of the flyout page based on the current device orientation.</summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void UpdateFlyoutLayoutBehavior()
		{
			UpdateFlyoutLayoutBehavior(this);
		}

		internal static void UpdateFlyoutLayoutBehavior(FlyoutPage page)
		{
			if (page is IFlyoutPageController fpc && fpc.ShouldShowSplitMode)
			{
				page.SetValue(IsPresentedProperty, true);
				if (page.FlyoutLayoutBehavior != FlyoutLayoutBehavior.Default)
					fpc.CanChangeIsPresented = false;
			}
		}

		static void OnIsPresentedPropertyChanged(BindableObject sender, object oldValue, object newValue)
			=> ((FlyoutPage)sender).IsPresentedChanged?.Invoke(sender, EventArgs.Empty);

		static void OnIsPresentedPropertyChanging(BindableObject sender, object oldValue, object newValue)
		{
			if (sender is Maui.IElement element && element.IsShimmed())
			{
				if (sender is FlyoutPage fp && fp is IFlyoutPageController fpc && !fpc.CanChangeIsPresented)
					throw new InvalidOperationException(string.Format("Can't change IsPresented when setting {0}", fp.FlyoutLayoutBehavior));
			}
			else
			{
				if ((!(bool)newValue) && sender is IFlyoutPageController fpc && fpc.ShouldShowSplitMode && sender is FlyoutPage fp)
				{
					throw new InvalidOperationException(string.Format("Can't change IsPresented when setting {0}", fp.FlyoutLayoutBehavior));
				}
			}
		}

		static void OnFlyoutLayoutBehaviorPropertyChanged(BindableObject sender, object oldValue, object newValue)
		{
			var page = (FlyoutPage)sender;
			UpdateFlyoutLayoutBehavior(page);
		}

		static object GetDefaultValue(BindableObject bindable)
		{
			return DeviceInfo.Platform == DevicePlatform.macOS;
		}

		/// <summary>Initializes a new instance of the <see cref="FlyoutPage"/> class.</summary>
		public FlyoutPage()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<FlyoutPage>>(() => new PlatformConfigurationRegistry<FlyoutPage>(this));
			(this as IControlsVisualElement).WindowChanged += OnWindowChanged;
			this.SizeChanged += OnSizeChanged;
		}

		readonly Lazy<PlatformConfigurationRegistry<FlyoutPage>> _platformConfigurationRegistry;

		/// <inheritdoc/>
		public new IPlatformElementConfiguration<T, FlyoutPage> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		void OnSizeChanged(object sender, EventArgs e)
		{
			if (Handler is not null)
			{
				Handler?.UpdateValue(nameof(FlyoutBehavior));
				SizeChanged -= OnSizeChanged;
			}
		}

		void OnWindowChanged(object sender, EventArgs e)
		{
			if (Window is null)
			{
				SizeChanged -= OnSizeChanged;
				SizeChanged += OnSizeChanged;
				DeviceDisplay.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;
			}
			else
			{
				DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
			}
		}

		void OnMainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
		{
			Handler?.UpdateValue(nameof(FlyoutBehavior));
		}

		IView IFlyoutView.Flyout => this.Flyout;
		IView IFlyoutView.Detail => this.Detail;

		Maui.FlyoutBehavior IFlyoutView.FlyoutBehavior
		{
			get
			{
				if (((IFlyoutPageController)this).ShouldShowSplitMode)
					return Maui.FlyoutBehavior.Locked;

				return Maui.FlyoutBehavior.Flyout;
			}
		}

#if ANDROID

		const double DefaultFlyoutSize = 320;
		const double DefaultSmallFlyoutSize = 240;

		double IFlyoutView.FlyoutWidth
		{
			get
			{
				if (DeviceInfo.Idiom == DeviceIdiom.Phone)
					return -1;

				var scaledScreenSize = DeviceDisplay.MainDisplayInfo.GetScaledScreenSize();
				double w = scaledScreenSize.Width;
				return w < DefaultSmallFlyoutSize ? w : (w < DefaultFlyoutSize ? DefaultSmallFlyoutSize : DefaultFlyoutSize);
			}
		}
#else
		double IFlyoutView.FlyoutWidth => -1;
#endif
		private protected override string GetDebuggerDisplay()
		{
			var debugText = DebuggerDisplayHelpers.GetDebugText(nameof(Detail), Detail, "FlyoutPage", Flyout, nameof(BindingContext), BindingContext);
			return $"{GetType().FullName}: {debugText}";
		}
	}
}
