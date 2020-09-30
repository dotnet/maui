using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.Core.Widget;
using AndroidX.DrawerLayout.Widget;
using AndroidX.Fragment.App;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	using Xamarin.Forms.Platform.Android.AppCompat;

	public class FlyoutPageRenderer : DrawerLayout, IVisualElementRenderer, DrawerLayout.IDrawerListener, IManageFragments, ILifeCycleState
	{
		#region Statics

		//from Android source code
		const uint DefaultScrimColor = 0x99000000;

		#endregion

		int _currentLockMode = -1;
		FlyoutPageContainer _detailLayout;
		FlyoutPageContainer _flyoutLayout;
		bool _disposed;
		bool _isPresentingFromCore;
		bool _presented;
		VisualElementTracker _tracker;
		FragmentManager _fragmentManager;
		string _defaultContentDescription;
		string _defaultHint;

		public FlyoutPageRenderer(Context context) : base(context)
		{
		}

		FlyoutPage Element { get; set; }

		IFlyoutPageController FlyoutPageController => Element as IFlyoutPageController;

		bool Presented
		{
			get { return _presented; }
			set
			{
				if (value == _presented)
					return;
				UpdateSplitViewLayout();
				_presented = value;
				if (Element.FlyoutLayoutBehavior == FlyoutLayoutBehavior.Default && FlyoutPageController.ShouldShowSplitMode)
					return;
				if (_presented)
					OpenDrawer(_flyoutLayout);
				else
					CloseDrawer(_flyoutLayout);
			}
		}

		IPageController FlyoutPagePageController => Element.Flyout as IPageController;
		IPageController DetailPageController => Element.Detail as IPageController;
		IPageController PageController => Element as IPageController;

		void IDrawerListener.OnDrawerClosed(global::Android.Views.View drawerView)
		{
		}

		void IDrawerListener.OnDrawerOpened(global::Android.Views.View drawerView)
		{
		}

		void IDrawerListener.OnDrawerSlide(global::Android.Views.View drawerView, float slideOffset)
		{
		}

		void IDrawerListener.OnDrawerStateChanged(int newState)
		{
			_presented = IsDrawerVisible(_flyoutLayout);
			UpdateIsPresented();
		}

		void IManageFragments.SetFragmentManager(FragmentManager fragmentManager)
		{
			if (_fragmentManager == null)
				_fragmentManager = fragmentManager;
		}

		VisualElement IVisualElementRenderer.Element => Element;

		event EventHandler<VisualElementChangedEventArgs> IVisualElementRenderer.ElementChanged
		{
			add { ElementChanged += value; }
			remove { ElementChanged -= value; }
		}

		event EventHandler<PropertyChangedEventArgs> IVisualElementRenderer.ElementPropertyChanged
		{
			add { ElementPropertyChanged += value; }
			remove { ElementPropertyChanged -= value; }
		}

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			Measure(widthConstraint, heightConstraint);
			return new SizeRequest(new Size(MeasuredWidth, MeasuredHeight));
		}

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			FlyoutPage oldElement = Element;
			FlyoutPage newElement = Element = element as FlyoutPage;

			if (oldElement != null)
			{
				Device.Info.PropertyChanged -= DeviceInfoPropertyChanged;

				((IFlyoutPageController)oldElement).BackButtonPressed -= OnBackButtonPressed;

				oldElement.PropertyChanged -= HandlePropertyChanged;
				oldElement.Appearing -= FlyoutPageAppearing;
				oldElement.Disappearing -= FlyoutPageDisappearing;

				RemoveDrawerListener(this);

				if (_detailLayout != null)
				{
					RemoveView(_detailLayout);
				}

				if (_flyoutLayout != null)
				{
					RemoveView(_flyoutLayout);
				}
			}

			if (newElement != null)
			{
				if (_detailLayout == null)
				{
					_detailLayout = new FlyoutPageContainer(newElement, false, Context)
					{
						LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
					};

					_flyoutLayout = new FlyoutPageContainer(newElement, true, Context)
					{
						LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent) { Gravity = (int)GravityFlags.Start }
					};

					if (_fragmentManager != null)
					{
						_detailLayout.SetFragmentManager(_fragmentManager);
						_flyoutLayout.SetFragmentManager(_fragmentManager);
					}

					AddView(_detailLayout);
					AddView(_flyoutLayout);

					Device.Info.PropertyChanged += DeviceInfoPropertyChanged;

					AddDrawerListener(this);
				}

				UpdateBackgroundColor(newElement);
				UpdateBackgroundImage(newElement);

				UpdateFlyout();
				UpdateDetail();

				UpdateFlowDirection();

				((IFlyoutPageController)newElement).BackButtonPressed += OnBackButtonPressed;
				newElement.PropertyChanged += HandlePropertyChanged;
				newElement.Appearing += FlyoutPageAppearing;
				newElement.Disappearing += FlyoutPageDisappearing;

				SetGestureState();

				Presented = newElement.IsPresented;

				newElement.SendViewInitialized(this);
			}

			OnElementChanged(oldElement, newElement);

			// Make sure to initialize this AFTER event is fired
			if (_tracker == null)
				_tracker = new VisualElementTracker(this);

			if (element != null && !string.IsNullOrEmpty(element.AutomationId))
				SetAutomationId(element.AutomationId);

			SetContentDescription();
		}

		void IVisualElementRenderer.SetLabelFor(int? id) => LabelFor = id ?? LabelFor;

		VisualElementTracker IVisualElementRenderer.Tracker => _tracker;

		void IVisualElementRenderer.UpdateLayout()
		{
			_tracker?.UpdateLayout();
		}

		ViewGroup IVisualElementRenderer.ViewGroup => this;

		AView IVisualElementRenderer.View => this;

		bool ILifeCycleState.MarkedForDispose { get; set; } = false;

		protected virtual void SetAutomationId(string id) => FastRenderers.AutomationPropertiesProvider.SetAutomationId(this, Element, id);

		protected virtual void SetContentDescription() => FastRenderers.AutomationPropertiesProvider.SetContentDescription(this, Element, ref _defaultContentDescription, ref _defaultHint);

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				Device.Info.PropertyChanged -= DeviceInfoPropertyChanged;

				if (Element != null)
				{
					FlyoutPageController.BackButtonPressed -= OnBackButtonPressed;
					Element.PropertyChanged -= HandlePropertyChanged;
					Element.Appearing -= FlyoutPageAppearing;
					Element.Disappearing -= FlyoutPageDisappearing;
				}

				if (_flyoutLayout?.ChildView != null)
					_flyoutLayout.ChildView.PropertyChanged -= HandleFlyoutPropertyChanged;

				if (!this.IsDisposed())
					RemoveDrawerListener(this);

				if (_tracker != null)
				{
					_tracker.Dispose();
					_tracker = null;
				}

				if (_detailLayout != null)
				{
					RemoveView(_detailLayout);
					_detailLayout.Dispose();
					_detailLayout = null;
				}

				if (_flyoutLayout != null)
				{
					RemoveView(_flyoutLayout);
					_flyoutLayout.Dispose();
					_flyoutLayout = null;
				}

				if (Element != null)
				{
					Element.ClearValue(Android.Platform.RendererProperty);
					Element = null;
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
			PageController?.SendDisappearing();
		}

		protected virtual void OnElementChanged(VisualElement oldElement, VisualElement newElement)
		{
			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(oldElement, newElement));
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			base.OnLayout(changed, l, t, r, b);
			//hack to make the split layout handle touches the full width
			if (FlyoutPageController.ShouldShowSplitMode && _flyoutLayout != null)
				_flyoutLayout.Right = r;
		}

		async void DeviceInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (nameof(Device.Info.CurrentOrientation) == e.PropertyName)
			{
				if (!FlyoutPageController.ShouldShowSplitMode && Presented)
				{
					FlyoutPageController.CanChangeIsPresented = true;
					//hack : when the orientation changes and we try to close the Flyout on Android		
					//sometimes Android picks the width of the screen previous to the rotation 		
					//this leaves a little of the flyout visible, the hack is to delay for 100ms closing the drawer
					await Task.Delay(100);

					//Renderer may have been disposed during the delay
					if (_disposed)
					{
						return;
					}

					CloseDrawer(_flyoutLayout);
				}

				UpdateSplitViewLayout();
			}
		}

		event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		bool HasAncestorNavigationPage(Element element)
		{
			if (element.Parent == null)
				return false;
			else if (element.Parent is NavigationPage)
				return true;
			else
				return HasAncestorNavigationPage(element.Parent);
		}

		void HandleFlyoutPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
		}

		void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ElementPropertyChanged?.Invoke(this, e);
			if (e.PropertyName == "Flyout")
				UpdateFlyout();
			else if (e.PropertyName == "Detail")
				UpdateDetail();
			else if (e.PropertyName == FlyoutPage.IsGestureEnabledProperty.PropertyName)
				SetGestureState();
			else if (e.PropertyName == FlyoutPage.IsPresentedProperty.PropertyName)
			{
				_isPresentingFromCore = true;
				Presented = Element.IsPresented;
				_isPresentingFromCore = false;
			}
			else if (e.PropertyName == Page.BackgroundImageSourceProperty.PropertyName)
				UpdateBackgroundImage(Element);
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor(Element);
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
		}

		void FlyoutPageAppearing(object sender, EventArgs e)
		{
			FlyoutPagePageController?.SendAppearing();
			DetailPageController?.SendAppearing();
		}

		void FlyoutPageDisappearing(object sender, EventArgs e)
		{
			FlyoutPagePageController?.SendDisappearing();
			DetailPageController?.SendDisappearing();
		}

		void OnBackButtonPressed(object sender, BackButtonPressedEventArgs backButtonPressedEventArgs)
		{
			if (!IsDrawerOpen((int)GravityFlags.Start) || _currentLockMode == LockModeLockedOpen)
				return;

			CloseDrawer((int)GravityFlags.Start);
			backButtonPressedEventArgs.Handled = true;
		}

		void SetGestureState()
		{
			SetDrawerLockMode(Element.IsGestureEnabled ? LockModeUnlocked : LockModeLockedClosed);
		}

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
			Color backgroundColor = view.BackgroundColor;
			if (backgroundColor.IsDefault)
				SetBackgroundColor(backgroundColor.ToAndroid());
		}

		void UpdateBackgroundImage(Page view)
		{
			_ = this.ApplyDrawableAsync(view, Page.BackgroundImageSourceProperty, Context, drawable =>
			{
				if (drawable != null)
					this.SetBackground(drawable);
			});
		}

		void UpdateDetail()
		{
			if (_detailLayout.ChildView == null)
				Update();
			else
				// Queue up disposal of the previous renderers after the current layout updates have finished
				new Handler(Looper.MainLooper).Post(Update);

			void Update()
			{
				if (_detailLayout == null || _detailLayout.IsDisposed())
					return;

				Context.HideKeyboard(this);
				_detailLayout.ChildView = Element.Detail;
			}
		}

		void UpdateFlowDirection()
		{
			this.UpdateFlowDirection(Element);
			_detailLayout.UpdateFlowDirection();
		}

		void UpdateIsPresented()
		{
			if (_isPresentingFromCore)
				return;
			if (Presented != Element.IsPresented)
				((IElementController)Element).SetValueFromRenderer(FlyoutPage.IsPresentedProperty, Presented);
		}

		void UpdateFlyout()
		{
			if (_flyoutLayout.ChildView == null)
				Update();
			else
				// Queue up disposal of the previous renderers after the current layout updates have finished
				new Handler(Looper.MainLooper).Post(Update);

			void Update()
			{
				if (_flyoutLayout == null || _flyoutLayout.IsDisposed())
					return;

				if (_flyoutLayout.ChildView != null)
					_flyoutLayout.ChildView.PropertyChanged -= HandleFlyoutPropertyChanged;

				_flyoutLayout.ChildView = Element.Flyout;

				if (_flyoutLayout.ChildView != null)
					_flyoutLayout.ChildView.PropertyChanged += HandleFlyoutPropertyChanged;
			}
		}

		void UpdateSplitViewLayout()
		{
			if (Device.Idiom == TargetIdiom.Tablet)
			{
				bool isShowingSplit = FlyoutPageController.ShouldShowSplitMode || (FlyoutPageController.ShouldShowSplitMode && Element.FlyoutLayoutBehavior != FlyoutLayoutBehavior.Default && Element.IsPresented);
				SetLockMode(isShowingSplit ? LockModeLockedOpen : LockModeUnlocked);
				unchecked
				{
					SetScrimColor(isShowingSplit ? Color.Transparent.ToAndroid() : (int)DefaultScrimColor);
				}
			}
		}
	}
}

namespace Xamarin.Forms.Platform.Android.AppCompat
{
	public class MasterDetailPageRenderer : FlyoutPageRenderer
	{
		public MasterDetailPageRenderer(Context context) : base(context)
		{
		}


		[Obsolete("This constructor is obsolete as of version 2.5. Please use FlyoutPageRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public MasterDetailPageRenderer() : base(Forms.Context)
		{
		}
	}
}