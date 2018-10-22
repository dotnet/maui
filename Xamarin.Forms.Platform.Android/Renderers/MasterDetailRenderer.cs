using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Support.V4.Widget;
using Android.Views;
using AView = Android.Views.View;
using AColor = Android.Graphics.Drawables.ColorDrawable;
using Android.OS;
using Xamarin.Forms.Platform.Android.FastRenderers;

namespace Xamarin.Forms.Platform.Android
{
	public class MasterDetailRenderer : DrawerLayout, IVisualElementRenderer, DrawerLayout.IDrawerListener
	{
		//from Android source code
		const uint DefaultScrimColor = 0x99000000;
		int _currentLockMode = -1;
		MasterDetailContainer _detailLayout;
		bool _isPresentingFromCore;
		MasterDetailContainer _masterLayout;
		MasterDetailPage _page;
		bool _presented;
		Platform _platform;

		string _defaultContentDescription;
		string _defaultHint;

		public MasterDetailRenderer(Context context) : base(context)
		{
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use MasterDetailRenderer(Context) instead.")]
		public MasterDetailRenderer() : base(Forms.Context)
		{
		}

		Platform Platform
		{
			get
			{
				if (_platform == null)
				{
					if (Context is FormsApplicationActivity activity)
					{
						_platform = activity.Platform;
					}
				}

				return _platform;
			}
		}

		IMasterDetailPageController MasterDetailPageController => _page;

		public bool Presented
		{
			get { return _presented; }
			set
			{
				if (value == _presented)
					return;
				UpdateSplitViewLayout();
				_presented = value;
				if (_page.MasterBehavior == MasterBehavior.Default && MasterDetailPageController.ShouldShowSplitMode)
					return;
				if (_presented)
					OpenDrawer(_masterLayout);
				else
					CloseDrawer(_masterLayout);
			}
		}

		IPageController MasterPageController => _page.Master as IPageController;
		IPageController DetailPageController => _page.Detail as IPageController;
		IPageController PageController => Element as IPageController;

		public void OnDrawerClosed(AView drawerView)
		{
		}

		public void OnDrawerOpened(AView drawerView)
		{
		}

		public void OnDrawerSlide(AView drawerView, float slideOffset)
		{
		}

		public void OnDrawerStateChanged(int newState)
		{
			_presented = IsDrawerVisible(_masterLayout);
			UpdateIsPresented();
		}

		public VisualElement Element
		{
			get { return _page; }
		}

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;
		event EventHandler<PropertyChangedEventArgs> IVisualElementRenderer.ElementPropertyChanged
		{
			add { ElementPropertyChanged += value; }
			remove { ElementPropertyChanged -= value; }
		}

		public SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			Measure(widthConstraint, heightConstraint);
			return new SizeRequest(new Size(MeasuredWidth, MeasuredHeight));
		}

		public void SetElement(VisualElement element)
		{
			MasterDetailPage oldElement = _page;
			_page = element as MasterDetailPage;

			_detailLayout = new MasterDetailContainer(_page, false, Context) { LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent) };

			_masterLayout = new MasterDetailContainer(_page, true, Context)
			{
				LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent) { Gravity = (int)GravityFlags.Start }
			};

			AddView(_detailLayout);

			AddView(_masterLayout);

			var activity = Context as Activity;
			activity?.ActionBar?.SetDisplayShowHomeEnabled(true);
			activity?.ActionBar?.SetHomeButtonEnabled(true);

			UpdateBackgroundColor(_page);
			UpdateBackgroundImage(_page);

			OnElementChanged(oldElement, element);

			if (oldElement != null)
				((IMasterDetailPageController)oldElement).BackButtonPressed -= OnBackButtonPressed;

			if (_page != null)
				MasterDetailPageController.BackButtonPressed += OnBackButtonPressed;

			if (Tracker == null)
				Tracker = new VisualElementTracker(this);

			_page.PropertyChanged += HandlePropertyChanged;
			_page.Appearing += MasterDetailPageAppearing;
			_page.Disappearing += MasterDetailPageDisappearing;

			UpdateMaster();
			UpdateDetail();

			Device.Info.PropertyChanged += DeviceInfoPropertyChanged;
			SetGestureState();

			Presented = _page.IsPresented;

			AddDrawerListener(this);

			if (element != null)
				element.SendViewInitialized(this);

			if (element != null && !string.IsNullOrEmpty(element.AutomationId))
					SetAutomationId(element.AutomationId);

			SetContentDescription();
		}

		protected virtual void SetAutomationId(string id)
		=> AutomationPropertiesProvider.SetAutomationId(this, Element, id);

		protected virtual void SetContentDescription()
			=> AutomationPropertiesProvider.SetContentDescription(this, Element, ref _defaultContentDescription, ref _defaultHint);


		public VisualElementTracker Tracker { get; private set; }

		public void UpdateLayout()
		{
			if (Tracker != null)
				Tracker.UpdateLayout();
		}

		public ViewGroup ViewGroup => this;
		AView IVisualElementRenderer.View => this;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Tracker != null)
				{
					Tracker.Dispose();
					Tracker = null;
				}

				if (_detailLayout != null)
				{
					_detailLayout.Dispose();
					_detailLayout = null;
				}

				if (_masterLayout != null)
				{
					_masterLayout.Dispose();
					_masterLayout = null;
				}

				Device.Info.PropertyChanged -= DeviceInfoPropertyChanged;

				if (_page != null)
				{
					MasterDetailPageController.BackButtonPressed -= OnBackButtonPressed;
					_page.PropertyChanged -= HandlePropertyChanged;
					_page.Appearing -= MasterDetailPageAppearing;
					_page.Disappearing -= MasterDetailPageDisappearing;
					_page.ClearValue(Platform.RendererProperty);
					_page = null;
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();
			PageController.SendAppearing();
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();
			PageController.SendDisappearing();
		}

		protected virtual void OnElementChanged(VisualElement oldElement, VisualElement newElement)
		{
			EventHandler<VisualElementChangedEventArgs> changed = ElementChanged;
			if (changed != null)
				changed(this, new VisualElementChangedEventArgs(oldElement, newElement));
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			base.OnLayout(changed, l, t, r, b);
			//hack to make the split layout handle touches the full width
			if (MasterDetailPageController.ShouldShowSplitMode && _masterLayout != null)
				_masterLayout.Right = r;
		}

		async void DeviceInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "CurrentOrientation")
			{
				if (!MasterDetailPageController.ShouldShowSplitMode && Presented)
				{
					MasterDetailPageController.CanChangeIsPresented = true;
					//hack : when the orientation changes and we try to close the Master on Android		
					//sometimes Android picks the width of the screen previous to the rotation 		
					//this leaves a little of the master visible, the hack is to delay for 50ms closing the drawer
					await Task.Delay(50);
					CloseDrawer(_masterLayout);
				}
				UpdateSplitViewLayout();
			}
		}

		void HandleMasterPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Page.TitleProperty.PropertyName || e.PropertyName == Page.IconProperty.PropertyName)
				Platform?.UpdateMasterDetailToggle(true);
		}

		void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ElementPropertyChanged?.Invoke(this, e);
			if (e.PropertyName == "Master")
				UpdateMaster();
			else if (e.PropertyName == "Detail")
			{
				UpdateDetail();
				Platform?.UpdateActionBar();
			}
			else if (e.PropertyName == MasterDetailPage.IsPresentedProperty.PropertyName)
			{
				_isPresentingFromCore = true;
				Presented = _page.IsPresented;
				_isPresentingFromCore = false;
			}
			else if (e.PropertyName == "IsGestureEnabled")
				SetGestureState();
			else if (e.PropertyName == Page.BackgroundImageProperty.PropertyName)
				UpdateBackgroundImage(_page);
			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor(_page);
		}

		void MasterDetailPageAppearing(object sender, EventArgs e)
		{
			MasterPageController?.SendAppearing();
			DetailPageController?.SendAppearing();
		}

		void MasterDetailPageDisappearing(object sender, EventArgs e)
		{
			MasterPageController?.SendDisappearing();
			DetailPageController?.SendDisappearing();
		}

		void OnBackButtonPressed(object sender, BackButtonPressedEventArgs backButtonPressedEventArgs)
		{
			if (IsDrawerOpen((int)GravityFlags.Start))
			{
				if (_currentLockMode != LockModeLockedOpen)
				{
					CloseDrawer((int)GravityFlags.Start);
					backButtonPressedEventArgs.Handled = true;
				}
			}
		}

		void SetGestureState()
		{
			SetDrawerLockMode(_page.IsGestureEnabled ? LockModeUnlocked : LockModeLockedClosed);
		}

		void IVisualElementRenderer.SetLabelFor(int? id) => LabelFor = id ?? LabelFor;

		void SetLockMode(int lockMode)
		{
			if (_currentLockMode != lockMode)
			{
				SetDrawerLockMode(lockMode);
				_currentLockMode = lockMode;
			}
		}

		void UpdateBackgroundColor(Page view)
		{
			if (view.BackgroundColor != Color.Default)
				SetBackgroundColor(view.BackgroundColor.ToAndroid());
		}

		void UpdateBackgroundImage(Page view)
		{
			if (!string.IsNullOrEmpty(view.BackgroundImage))
				this.SetBackground(Context.GetDrawable(view.BackgroundImage));
		}

		void UpdateDetail()
		{
			if (_detailLayout.ChildView == null)
				Update();
			else
				// Queue up disposal of the previous renderers after the current layout updates have finished
				new Handler(Looper.MainLooper).Post(() => Update());

			void Update()
			{
				Context.HideKeyboard(this);
				_detailLayout.ChildView = _page.Detail;
			}
		}

		void UpdateIsPresented()
		{
			if (_isPresentingFromCore)
				return;
			if (Presented != _page.IsPresented)
				((IElementController)_page).SetValueFromRenderer(MasterDetailPage.IsPresentedProperty, Presented);
		}

		void UpdateMaster()
		{
			if (_masterLayout?.ChildView == null)
				Update();
			else
				// Queue up disposal of the previous renderers after the current layout updates have finished
				new Handler(Looper.MainLooper).Post(() => Update());

			void Update()
			{
				if (_masterLayout != null && _masterLayout.ChildView != null)
					_masterLayout.ChildView.PropertyChanged -= HandleMasterPropertyChanged;
				_masterLayout.ChildView = _page.Master;
				if (_page.Master != null)
					_page.Master.PropertyChanged += HandleMasterPropertyChanged;
			}
		}

		void UpdateSplitViewLayout()
		{
			if (Device.Idiom == TargetIdiom.Tablet)
			{
				bool isShowingSplit = MasterDetailPageController.ShouldShowSplitMode
					|| (MasterDetailPageController.ShouldShowSplitMode && _page.MasterBehavior != MasterBehavior.Default && _page.IsPresented);
				SetLockMode(isShowingSplit ? LockModeLockedOpen : LockModeUnlocked);
				unchecked
				{
					SetScrimColor(isShowingSplit ? Color.Transparent.ToAndroid() : (int)DefaultScrimColor);
				}
				Platform?.UpdateMasterDetailToggle();
			}
		}
	}
}
