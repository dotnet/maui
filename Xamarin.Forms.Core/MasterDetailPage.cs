using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[RenderWith(typeof(_MasterDetailPageRenderer))]
	public class MasterDetailPage : Page, IMasterDetailPageController, IElementConfiguration<MasterDetailPage>
	{
		public static readonly BindableProperty IsGestureEnabledProperty = BindableProperty.Create("IsGestureEnabled", typeof(bool), typeof(MasterDetailPage), true);

		public static readonly BindableProperty IsPresentedProperty = BindableProperty.Create("IsPresented", typeof(bool), typeof(MasterDetailPage),default(bool),
			propertyChanged: OnIsPresentedPropertyChanged, propertyChanging: OnIsPresentedPropertyChanging, defaultValueCreator : GetDefaultValue);

		public static readonly BindableProperty MasterBehaviorProperty = BindableProperty.Create("MasterBehavior", typeof(MasterBehavior), typeof(MasterDetailPage), default(MasterBehavior),
			propertyChanged: OnMasterBehaviorPropertyChanged);

		Page _detail;

		Rectangle _detailBounds;

		Page _master;

		Rectangle _masterBounds;

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

		public Page Master
		{
			get { return _master; }
			set
			{
				if (_master != null && value == null)
					throw new ArgumentNullException("value", "Master cannot be set to null once a value is set");

				if (string.IsNullOrEmpty(value.Title))
					throw new InvalidOperationException("Title property must be set on Master page");

				if (_master == value)
					return;

				if (value.RealParent != null)
					throw new InvalidOperationException("Master must not already have a parent.");

				OnPropertyChanging();
				if (_master != null)
					InternalChildren.Remove(_master);
				_master = value;
				InternalChildren.Add(_master);
				OnPropertyChanged();
			}
		}

		public MasterBehavior MasterBehavior
		{
			get { return (MasterBehavior)GetValue(MasterBehaviorProperty); }
			set { SetValue(MasterBehaviorProperty, value); }
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
					throw new InvalidOperationException("Detail must be set before using a MasterDetailPage");
				_detail.Layout(value);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public Rectangle MasterBounds
		{
			get { return _masterBounds; }
			set
			{
				_masterBounds = value;
				if (_master == null)
					throw new InvalidOperationException("Master must be set before using a MasterDetailPage");
				_master.Layout(value);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldShowSplitMode
		{
			get
			{
				if (Device.Idiom == TargetIdiom.Phone)
					return false;

				MasterBehavior behavior = MasterBehavior;
				DeviceOrientation orientation = Device.Info.CurrentOrientation;

				bool isSplitOnLandscape = (behavior == MasterBehavior.SplitOnLandscape || behavior == MasterBehavior.Default) && orientation.IsLandscape();
				bool isSplitOnPortrait = behavior == MasterBehavior.SplitOnPortrait && orientation.IsPortrait();
				return behavior == MasterBehavior.Split || isSplitOnLandscape || isSplitOnPortrait;
			}
		}

		public event EventHandler IsPresentedChanged;

		public virtual bool ShouldShowToolbarButton()
		{
			if (Device.Idiom == TargetIdiom.Phone)
				return true;

			MasterBehavior behavior = MasterBehavior;
			DeviceOrientation orientation = Device.Info.CurrentOrientation;

			bool isSplitOnLandscape = (behavior == MasterBehavior.SplitOnLandscape || behavior == MasterBehavior.Default) && orientation.IsLandscape();
			bool isSplitOnPortrait = behavior == MasterBehavior.SplitOnPortrait && orientation.IsPortrait();
			return behavior != MasterBehavior.Split && !isSplitOnLandscape && !isSplitOnPortrait;
		}

		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			if (Master == null || Detail == null)
				throw new InvalidOperationException("Master and Detail must be set before using a MasterDetailPage");
			_master.Layout(_masterBounds);
			_detail.Layout(_detailBounds);
		}

		protected override void OnAppearing()
		{
			CanChangeIsPresented = true;
			UpdateMasterBehavior(this);
			base.OnAppearing();
		}

		protected override bool OnBackButtonPressed()
		{
			if (IsPresented)
			{
				if (Master.SendBackButtonPressed())
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
			if (RealParent != null && (Master == null || Detail == null))
				throw new InvalidOperationException("Master and Detail must be set before adding MasterDetailPage to a container");
			base.OnParentSet();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<BackButtonPressedEventArgs> BackButtonPressed;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void UpdateMasterBehavior()
		{
			UpdateMasterBehavior(this);
		}

		internal static void UpdateMasterBehavior(MasterDetailPage page)
		{
			if (page.ShouldShowSplitMode)
			{
				page.SetValueCore(IsPresentedProperty, true);
				if (page.MasterBehavior != MasterBehavior.Default)
					page.CanChangeIsPresented = false;
			}
		}

		static void OnIsPresentedPropertyChanged(BindableObject sender, object oldValue, object newValue)
		{
			var page = (MasterDetailPage)sender;
			EventHandler handler = page.IsPresentedChanged;
			if (handler != null)
				handler(page, EventArgs.Empty);
		}

		static void OnIsPresentedPropertyChanging(BindableObject sender, object oldValue, object newValue)
		{
			var page = (MasterDetailPage)sender;
			if (!page.CanChangeIsPresented)
				throw new InvalidOperationException(string.Format("Can't change IsPresented when setting {0}", page.MasterBehavior));
		}

		static void OnMasterBehaviorPropertyChanged(BindableObject sender, object oldValue, object newValue)
		{
			var page = (MasterDetailPage)sender;
			UpdateMasterBehavior(page);
		}

		static object GetDefaultValue(BindableObject bindable)
		{
			return Device.RuntimePlatform == Device.macOS;
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
	}
}