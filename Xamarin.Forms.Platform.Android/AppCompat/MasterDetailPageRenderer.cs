using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content;
#if __ANDROID_29__
using AndroidX.Core.Widget;
using AndroidX.Fragment.App;
using AndroidX.DrawerLayout.Widget;
#else
using Android.Support.V4.Widget;
using Android.Support.V4.App;
#endif
using Android.Views;
using AView = Android.Views.View;
using Android.OS;

namespace Xamarin.Forms.Platform.Android.AppCompat
{
	public class MasterDetailPageRenderer : DrawerLayout, IVisualElementRenderer, DrawerLayout.IDrawerListener, IManageFragments, ILifeCycleState
	{
		#region Statics

		//from Android source code
		const uint DefaultScrimColor = 0x99000000;

		#endregion

		int _currentLockMode = -1;
		MasterDetailContainer _detailLayout;
		MasterDetailContainer _masterLayout;
		bool _disposed;
		bool _isPresentingFromCore;
		bool _presented;
		VisualElementTracker _tracker;
		FragmentManager _fragmentManager;
		string _defaultContentDescription;
		string _defaultHint;

		public MasterDetailPageRenderer(Context context) : base(context)
		{
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use MasterDetailPageRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public MasterDetailPageRenderer() : base(Forms.Context)
		{
		}

		MasterDetailPage Element { get; set; }

		IMasterDetailPageController MasterDetailPageController => Element as IMasterDetailPageController;

		bool Presented
		{
			get { return _presented; }
			set
			{
				if (value == _presented)
					return;
				UpdateSplitViewLayout();
				_presented = value;
				if (Element.MasterBehavior == MasterBehavior.Default && MasterDetailPageController.ShouldShowSplitMode)
					return;
				if (_presented)
					OpenDrawer(_masterLayout);
				else
					CloseDrawer(_masterLayout);
			}
		}

		IPageController MasterPageController => Element.Master as IPageController;
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
			_presented = IsDrawerVisible(_masterLayout);
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
			MasterDetailPage oldElement = Element;
			MasterDetailPage newElement = Element = element as MasterDetailPage;

			if (oldElement != null)
			{
				Device.Info.PropertyChanged -= DeviceInfoPropertyChanged;

				((IMasterDetailPageController)oldElement).BackButtonPressed -= OnBackButtonPressed;

				oldElement.PropertyChanged -= HandlePropertyChanged;
				oldElement.Appearing -= MasterDetailPageAppearing;
				oldElement.Disappearing -= MasterDetailPageDisappearing;

				RemoveDrawerListener(this);
			
				if (_detailLayout != null)
				{
					RemoveView(_detailLayout);
				}

				if (_masterLayout != null)
				{
					RemoveView(_masterLayout);
				}
			}

			if (newElement != null)
			{
				if (_detailLayout == null)
				{
					_detailLayout = new MasterDetailContainer(newElement, false, Context)
					{
						LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
					};

					_masterLayout = new MasterDetailContainer(newElement, true, Context)
					{
						LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent) { Gravity = (int)GravityFlags.Start }
					};

					if (_fragmentManager != null)
					{
						_detailLayout.SetFragmentManager(_fragmentManager);
						_masterLayout.SetFragmentManager(_fragmentManager);
					}

					AddView(_detailLayout);
					AddView(_masterLayout);

					Device.Info.PropertyChanged += DeviceInfoPropertyChanged;

					AddDrawerListener(this);
				}

				UpdateBackgroundColor(newElement);
				UpdateBackgroundImage(newElement);

				UpdateMaster();
				UpdateDetail();

				UpdateFlowDirection();

				((IMasterDetailPageController)newElement).BackButtonPressed += OnBackButtonPressed;
				newElement.PropertyChanged += HandlePropertyChanged;
				newElement.Appearing += MasterDetailPageAppearing;
				newElement.Disappearing += MasterDetailPageDisappearing;

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
					MasterDetailPageController.BackButtonPressed -= OnBackButtonPressed;
					Element.PropertyChanged -= HandlePropertyChanged;
					Element.Appearing -= MasterDetailPageAppearing;
					Element.Disappearing -= MasterDetailPageDisappearing;
				}

				if (_masterLayout?.ChildView != null)
					_masterLayout.ChildView.PropertyChanged -= HandleMasterPropertyChanged;

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

				if (_masterLayout != null)
				{
					RemoveView(_masterLayout);
					_masterLayout.Dispose();
					_masterLayout = null;
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
			if (MasterDetailPageController.ShouldShowSplitMode && _masterLayout != null)
				_masterLayout.Right = r;
		}

		async void DeviceInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (nameof(Device.Info.CurrentOrientation) == e.PropertyName)
			{
				if (!MasterDetailPageController.ShouldShowSplitMode && Presented)
				{
					MasterDetailPageController.CanChangeIsPresented = true;
					//hack : when the orientation changes and we try to close the Master on Android		
					//sometimes Android picks the width of the screen previous to the rotation 		
					//this leaves a little of the master visible, the hack is to delay for 100ms closing the drawer
					await Task.Delay(100);

					//Renderer may have been disposed during the delay
					if (_disposed)
					{
						return;
					}

					CloseDrawer(_masterLayout);
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

		void HandleMasterPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
		}

		void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ElementPropertyChanged?.Invoke(this, e);
			if (e.PropertyName == "Master")
				UpdateMaster();
			else if (e.PropertyName == "Detail")
				UpdateDetail();
			else if (e.PropertyName == MasterDetailPage.IsGestureEnabledProperty.PropertyName)
				SetGestureState();
			else if (e.PropertyName == MasterDetailPage.IsPresentedProperty.PropertyName)
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
				((IElementController)Element).SetValueFromRenderer(MasterDetailPage.IsPresentedProperty, Presented);
		}

		void UpdateMaster()
		{
			if (_masterLayout.ChildView == null)
				Update();
			else
				// Queue up disposal of the previous renderers after the current layout updates have finished
				new Handler(Looper.MainLooper).Post(Update);

			void Update()
			{
				if (_masterLayout == null || _masterLayout.IsDisposed())
					return;

				if (_masterLayout.ChildView != null)
					_masterLayout.ChildView.PropertyChanged -= HandleMasterPropertyChanged;

				_masterLayout.ChildView = Element.Master;

				if (_masterLayout.ChildView != null)
					_masterLayout.ChildView.PropertyChanged += HandleMasterPropertyChanged;
			}
		}

		void UpdateSplitViewLayout()
		{
			if (Device.Idiom == TargetIdiom.Tablet)
			{
				bool isShowingSplit = MasterDetailPageController.ShouldShowSplitMode || (MasterDetailPageController.ShouldShowSplitMode && Element.MasterBehavior != MasterBehavior.Default && Element.IsPresented);
				SetLockMode(isShowingSplit ? LockModeLockedOpen : LockModeUnlocked);
				unchecked
				{
					SetScrimColor(isShowingSplit ? Color.Transparent.ToAndroid() : (int)DefaultScrimColor);
				}
			}
		}
	}
}