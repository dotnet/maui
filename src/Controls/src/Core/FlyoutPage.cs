using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	[ContentProperty("Detail")]
	public class FlyoutPage : Page, IFlyoutPageController, IElementConfiguration<FlyoutPage>
	{
		public static readonly BindableProperty IsGestureEnabledProperty = BindableProperty.Create(nameof(IsGestureEnabled), typeof(bool), typeof(FlyoutPage), true);

		public static readonly BindableProperty IsPresentedProperty = BindableProperty.Create(nameof(IsPresented), typeof(bool), typeof(FlyoutPage), default(bool),
			propertyChanged: OnIsPresentedPropertyChanged, propertyChanging: OnIsPresentedPropertyChanging, defaultValueCreator: GetDefaultValue);

		public static readonly BindableProperty FlyoutLayoutBehaviorProperty = BindableProperty.Create(nameof(FlyoutLayoutBehavior), typeof(FlyoutLayoutBehavior), typeof(FlyoutPage), default(FlyoutLayoutBehavior),
			propertyChanged: OnFlyoutLayoutBehaviorPropertyChanged);

		Page _detail;

		Rectangle _detailBounds;

		Page _flyout;

		Rectangle _flyoutBounds;

		public Page Detail
		{
			get { return _detail; }
			set
			{
				if (_detail != null && value == null)
					throw new ArgumentNullException("value", "Detail cannot be set to null once a value is set.");

				if (_detail == value)
					return;

				if (value.RealParent != null)
					throw new InvalidOperationException("Detail must not already have a parent.");

				OnPropertyChanging();
				if (_detail != null)
					InternalChildren.Remove(_detail);
				_detail = value;
				InternalChildren.Add(_detail);
				OnPropertyChanged();
			}
		}

		public bool IsGestureEnabled
		{
			get { return (bool)GetValue(IsGestureEnabledProperty); }
			set { SetValue(IsGestureEnabledProperty, value); }
		}

		public bool IsPresented
		{
			get { return (bool)GetValue(IsPresentedProperty); }
			set { SetValue(IsPresentedProperty, value); }
		}

		public Page Flyout
		{
			get { return _flyout; }
			set
			{
				if (_flyout != null && value == null)
					throw new ArgumentNullException("value", "Flyout cannot be set to null once a value is set");

				if (string.IsNullOrEmpty(value.Title))
					throw new InvalidOperationException("Title property must be set on Flyout page");

				if (_flyout == value)
					return;

				if (value.RealParent != null)
					throw new InvalidOperationException("Flyout must not already have a parent.");

				OnPropertyChanging();
				if (_flyout != null)
					InternalChildren.Remove(_flyout);
				_flyout = value;
				InternalChildren.Add(_flyout);
				OnPropertyChanged();
			}
		}

		public FlyoutLayoutBehavior FlyoutLayoutBehavior
		{
			get { return (FlyoutLayoutBehavior)GetValue(FlyoutLayoutBehaviorProperty); }
			set { SetValue(FlyoutLayoutBehaviorProperty, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool CanChangeIsPresented { get; set; } = true;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public Rectangle DetailBounds
		{
			get { return _detailBounds; }
			set
			{
				_detailBounds = value;
				if (_detail == null)
					throw new InvalidOperationException("Detail must be set before using a FlyoutPage");
				_detail.Layout(value);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public Rectangle FlyoutBounds
		{
			get { return _flyoutBounds; }
			set
			{
				_flyoutBounds = value;
				if (_flyout == null)
					throw new InvalidOperationException("Flyout must be set before using a FlyoutPage");
				_flyout.Layout(value);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldShowSplitMode
		{
			get
			{
				if (Device.Idiom == TargetIdiom.Phone)
					return false;

				FlyoutLayoutBehavior behavior = FlyoutLayoutBehavior;
				DeviceOrientation orientation = Device.Info.CurrentOrientation;

				bool isSplitOnLandscape = (behavior == FlyoutLayoutBehavior.SplitOnLandscape || behavior == FlyoutLayoutBehavior.Default) && orientation.IsLandscape();
				bool isSplitOnPortrait = behavior == FlyoutLayoutBehavior.SplitOnPortrait && orientation.IsPortrait();
				return behavior == FlyoutLayoutBehavior.Split || isSplitOnLandscape || isSplitOnPortrait;
			}
		}

		public event EventHandler IsPresentedChanged;

		public virtual bool ShouldShowToolbarButton()
		{
			if (Device.Idiom == TargetIdiom.Phone)
				return true;

			FlyoutLayoutBehavior behavior = FlyoutLayoutBehavior;
			DeviceOrientation orientation = Device.Info.CurrentOrientation;

			bool isSplitOnLandscape = (behavior == FlyoutLayoutBehavior.SplitOnLandscape || behavior == FlyoutLayoutBehavior.Default) && orientation.IsLandscape();
			bool isSplitOnPortrait = behavior == FlyoutLayoutBehavior.SplitOnPortrait && orientation.IsPortrait();
			return behavior != FlyoutLayoutBehavior.Split && !isSplitOnLandscape && !isSplitOnPortrait;
		}

		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			if (Flyout == null || Detail == null)
				throw new InvalidOperationException("Flyout and Detail must be set before using a FlyoutPage");
			_flyout.Layout(_flyoutBounds);
			_detail.Layout(_detailBounds);
		}

		protected override void OnAppearing()
		{
			CanChangeIsPresented = true;
			UpdateFlyoutLayoutBehavior(this);
			base.OnAppearing();
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

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void UpdateFlyoutLayoutBehavior()
		{
			UpdateFlyoutLayoutBehavior(this);
		}

		internal static void UpdateFlyoutLayoutBehavior(FlyoutPage page)
		{
			if (page.ShouldShowSplitMode)
			{
				page.SetValueCore(IsPresentedProperty, true);
				if (page.FlyoutLayoutBehavior != FlyoutLayoutBehavior.Default)
					page.CanChangeIsPresented = false;
			}
		}

		static void OnIsPresentedPropertyChanged(BindableObject sender, object oldValue, object newValue)
			=> ((FlyoutPage)sender).IsPresentedChanged?.Invoke(sender, EventArgs.Empty);

		static void OnIsPresentedPropertyChanging(BindableObject sender, object oldValue, object newValue)
		{
			var page = (FlyoutPage)sender;
			if (!page.CanChangeIsPresented)
				throw new InvalidOperationException(string.Format("Can't change IsPresented when setting {0}", page.FlyoutLayoutBehavior));
		}

		static void OnFlyoutLayoutBehaviorPropertyChanged(BindableObject sender, object oldValue, object newValue)
		{
			var page = (FlyoutPage)sender;
			UpdateFlyoutLayoutBehavior(page);
		}

		static object GetDefaultValue(BindableObject bindable)
		{
			return Device.RuntimePlatform == Device.macOS;
		}

		public FlyoutPage()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<FlyoutPage>>(() => new PlatformConfigurationRegistry<FlyoutPage>(this));
		}

		readonly Lazy<PlatformConfigurationRegistry<FlyoutPage>> _platformConfigurationRegistry;

		public new IPlatformElementConfiguration<T, FlyoutPage> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}
	}

	[Obsolete("MasterDetailPage is obsolete as of version 5.0.0. Please use FlyoutPage instead.")]
	public class MasterDetailPage : FlyoutPage, IMasterDetailPageController
	{
		public static readonly BindableProperty MasterBehaviorProperty = BindableProperty.Create(nameof(MasterBehavior), typeof(MasterBehavior), typeof(MasterDetailPage), default(MasterBehavior),
			propertyChanged: OnMasterBehaviorPropertyChanged);

		static void OnMasterBehaviorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if ((int)bindable.GetValue(FlyoutLayoutBehaviorProperty) != (int)newValue)
				bindable.SetValue(FlyoutLayoutBehaviorProperty, (FlyoutLayoutBehavior)((int)newValue));
		}

		public Page Master
		{
			get => base.Flyout;
			set => base.Flyout = value;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public Rectangle MasterBounds
		{
			get => base.FlyoutBounds;
			set => FlyoutBounds = value;
		}

		public MasterBehavior MasterBehavior
		{
			get => (MasterBehavior)GetValue(MasterBehaviorProperty);
			set => SetValue(MasterBehaviorProperty, value);
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);
			if (propertyName == nameof(Flyout))
				OnPropertyChanged(nameof(Master));

			if (propertyName == nameof(FlyoutLayoutBehavior))
				OnPropertyChanged(nameof(Flyout));
		}

		protected override void OnPropertyChanging([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanging(propertyName);
			if (propertyName == nameof(Flyout))
				OnPropertyChanging(nameof(Master));

			if (propertyName == nameof(FlyoutLayoutBehavior))
				OnPropertyChanging(nameof(Flyout));
		}


		public MasterDetailPage()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<MasterDetailPage>>(() => new PlatformConfigurationRegistry<MasterDetailPage>(this));
		}

		readonly Lazy<PlatformConfigurationRegistry<MasterDetailPage>> _platformConfigurationRegistry;

		public new IPlatformElementConfiguration<T, MasterDetailPage> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void UpdateMasterBehavior() =>
			(this as IFlyoutPageController).UpdateFlyoutLayoutBehavior();
	}
}