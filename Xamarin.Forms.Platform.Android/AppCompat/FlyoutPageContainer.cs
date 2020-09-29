using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Views;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android.AppCompat;
using Fragment = AndroidX.Fragment.App.Fragment;
using FragmentContainer = Xamarin.Forms.Platform.Android.AppCompat.FragmentContainer;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;
using FragmentTransaction = AndroidX.Fragment.App.FragmentTransaction;

namespace Xamarin.Forms.Platform.Android
{
	internal class FlyoutPageContainer : ViewGroup, IManageFragments
	{
		const int DefaultFlyoutSize = 320;
		const int DefaultSmallFlyoutSize = 240;
		readonly bool _isFlyout;
		VisualElement _childView;
		PageContainer _pageContainer;
		FragmentManager _fragmentManager;
		FlyoutPage _parent;
		Fragment _currentFragment;
		bool _disposed;
		FragmentTransaction _transaction;

		public FlyoutPageContainer(FlyoutPage parent, bool isFlyout, Context context) : base(context)
		{
			Id = Platform.GenerateViewId();
			_parent = parent;
			_isFlyout = isFlyout;
		}

		public bool MarkedForDispose { get; internal set; } = false;

		FragmentManager FragmentManager => _fragmentManager ?? (_fragmentManager = Context.GetFragmentManager());
		IFlyoutPageController FlyoutPageController => _parent as IFlyoutPageController;

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (_childView == null)
				return;

			Rectangle bounds = GetBounds(_isFlyout, l, t, r, b);
			if (_isFlyout)
				FlyoutPageController.FlyoutBounds = bounds;
			else
				FlyoutPageController.DetailBounds = bounds;

			IVisualElementRenderer renderer = Platform.GetRenderer(_childView);
			renderer?.UpdateLayout();

			// If we're using a PageContainer (i.e., we've wrapped our contents in a Fragment),
			// Make sure that it gets laid out
			if (_pageContainer != null)
			{
				if (_isFlyout)
				{
					var controller = (IFlyoutPageController)_parent;
					var width = (int)Context.ToPixels(controller.FlyoutBounds.Width);
					// When the base class computes the size of the Flyout container, it starts at the top of the 
					// screen and adds padding (_parent.FlyoutBounds.Top) to leave room for the status bar
					// When this container is laid out, it's already starting from the adjusted y value of the parent,
					// so we subtract _parent.FlyoutBounds.Top from our starting point (to get 0) and add it to the 
					// bottom (so the flyout page stretches to the bottom of the screen)
					var height = (int)Context.ToPixels(controller.FlyoutBounds.Height + controller.FlyoutBounds.Top);
					_pageContainer.Layout(0, 0, width, height);
				}
				else
				{
					_pageContainer.Layout(l, t, r, b);
				}

				_pageContainer.Child.UpdateLayout();
			}
		}

		public void UpdateFlowDirection() => _pageContainer?.UpdateFlowDirection(_parent);

		public VisualElement ChildView
		{
			get { return _childView; }
			set
			{
				if (_childView == value)
					return;

				RemoveAllViews();
				if (_childView != null)
					DisposeChildRenderers();

				_childView = value;

				if (_childView == null)
					return;

				AddChildView(_childView);
			}
		}

		protected virtual void AddChildView(VisualElement childView)
		{
			_pageContainer = null;

			Page page = childView as NavigationPage ?? (Page)(childView as TabbedPage);

			if (page == null)
			{
				// The thing we're adding is not a NavigationPage or TabbedPage, so we can just use the old AddChildView 

				if (_currentFragment != null)
				{
					if (!_parent.IsAttachedToRoot())
						return;

					// But first, if the previous occupant of this container was a fragment, we need to remove it properly
					FragmentTransaction transaction = FragmentManager.BeginTransactionEx();
					transaction.RemoveEx(_currentFragment);
					transaction.SetTransitionEx((int)FragmentTransit.None);

					if (IsAttachedToWindow)
						ExecuteTransaction(transaction);
					else
						_transaction = transaction;

					_currentFragment = null;
				}

				IVisualElementRenderer renderer = Platform.GetRenderer(childView);
				if (renderer == null)
					Platform.SetRenderer(childView, renderer = Platform.CreateRenderer(childView, Context));

				if (renderer.View.Parent != this)
				{
					if (renderer.View.Parent != null)
						renderer.View.RemoveFromParent();
					SetDefaultBackgroundColor(renderer);
					AddView(renderer.View);
					renderer.UpdateLayout();
				}
			}
			else
			{
				if (!_parent.IsAttachedToRoot())
					return;

				// The renderers for NavigationPage and TabbedPage both host fragments, so they need to be wrapped in a 
				// FragmentContainer in order to get isolated fragment management
				Fragment fragment = FragmentContainer.CreateInstance(page);

				var fc = fragment as FragmentContainer;

				fc?.SetOnCreateCallback(pc =>
				{
					_pageContainer = pc;
					UpdateFlowDirection();
					SetDefaultBackgroundColor(pc.Child);
				});

				FragmentTransaction transaction = FragmentManager.BeginTransactionEx();

				if (_currentFragment != null)
					transaction.RemoveEx(_currentFragment);

				transaction.AddEx(Id, fragment);
				transaction.SetTransitionEx((int)FragmentTransit.None);

				if (IsAttachedToWindow)
					ExecuteTransaction(transaction);
				else
					_transaction = transaction;

				_currentFragment = fragment;
			}
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			if (_transaction == null)
				return;

			ExecuteTransaction(_transaction);

			_transaction = null;
		}

		public int TopPadding { get; set; }

		double DefaultWidthFlyout
		{
			get
			{
				double w = Context.FromPixels(Resources.DisplayMetrics.WidthPixels);
				return w < DefaultSmallFlyoutSize ? w : (w < DefaultFlyoutSize ? DefaultSmallFlyoutSize : DefaultFlyoutSize);
			}
		}

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			bool isShowingPopover = _parent.IsPresented && !FlyoutPageController.ShouldShowSplitMode;
			if (!_isFlyout && isShowingPopover)
				return true;
			return base.OnInterceptTouchEvent(ev);
		}


		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				if (_currentFragment != null && !FragmentManager.IsDestroyed)
				{
					FragmentTransaction transaction = FragmentManager.BeginTransactionEx();
					transaction.RemoveEx(_currentFragment);
					transaction.SetTransitionEx((int)FragmentTransit.None);
					transaction.CommitAllowingStateLossEx();
					FragmentManager.ExecutePendingTransactionsEx();

					_currentFragment = null;
				}

				_parent = null;
				_pageContainer = null;
				_fragmentManager = null;
				RemoveAllViews();
				DisposeChildRenderers();
			}

			base.Dispose(disposing);
		}

		public void SetFragmentManager(FragmentManager fragmentManager)
		{
			if (_fragmentManager == null)
				_fragmentManager = fragmentManager;
		}

		void ExecuteTransaction(FragmentTransaction transaction)
		{
			// We don't currently support fragment restoration 
			// So we don't need to worry about loss of this fragment's state
			transaction.CommitAllowingStateLossEx();

			// The transaction need to be executed after View has been attached
			// So Fragment Manager can find the View being added
			FragmentManager.ExecutePendingTransactionsEx();
		}

		void DisposeChildRenderers()
		{
			IVisualElementRenderer childRenderer = Platform.GetRenderer(_childView);
			childRenderer?.Dispose();
			_childView?.ClearValue(Platform.RendererProperty);
		}

		Rectangle GetBounds(bool isFlyoutPage, int left, int top, int right, int bottom)
		{
			double width = Context.FromPixels(right - left);
			double height = Context.FromPixels(bottom - top);
			double xPos = 0;
			bool supressPadding = false;

			//splitview
			if (FlyoutPageController.ShouldShowSplitMode)
			{
				//to keep some behavior we have on iPad where you can toggle and it won't do anything 
				bool isDefaultNoToggle = _parent.FlyoutLayoutBehavior == FlyoutLayoutBehavior.Default;
				xPos = isFlyoutPage ? 0 : (_parent.IsPresented || isDefaultNoToggle ? DefaultWidthFlyout : 0);
				width = isFlyoutPage ? DefaultWidthFlyout : _parent.IsPresented || isDefaultNoToggle ? width - DefaultWidthFlyout : width;
			}
			else
			{
				//if we are showing the normal popover master doesn't have padding
				supressPadding = isFlyoutPage;
				//popover make the master smaller
				width = isFlyoutPage && (Device.Info.CurrentOrientation.IsLandscape() || Device.Idiom == TargetIdiom.Tablet) ? DefaultWidthFlyout : width;
			}

			double padding = supressPadding ? 0 : Context.FromPixels(TopPadding);
			return new Rectangle(xPos, padding, width, height - padding);
		}

		protected void SetDefaultBackgroundColor(IVisualElementRenderer renderer)
		{
			if (ChildView.BackgroundColor == Color.Default)
			{
				TypedArray colors = Context.Theme.ObtainStyledAttributes(new[] { global::Android.Resource.Attribute.ColorBackground });
				renderer.View.SetBackgroundColor(new global::Android.Graphics.Color(colors.GetColor(0, 0)));
			}
		}
	}
}