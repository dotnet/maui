using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using AndroidX.DrawerLayout.Widget;
using AndroidX.Fragment.App;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;
using ARect = Android.Graphics.Rect;
using AView = Android.Views.View;
using Color = Microsoft.Maui.Graphics.Color;
using LP = Android.Views.ViewGroup.LayoutParams;
using Paint = Android.Graphics.Paint;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Microsoft.Maui.Controls.Platform
{
	public class ShellView : IShellContext, IAppearanceObserver
	{

		#region IShellContext

		Context IShellContext.AndroidContext => AndroidContext;

		// This is very bad, FIXME.
		// This assumes all flyouts will implement via DrawerLayout which is PROBABLY true but
		// I dont want to back us into a corner this time.
		DrawerLayout IShellContext.CurrentDrawerLayout => (DrawerLayout)_flyoutView.AndroidView;

		Shell IShellContext.Shell => Element;

		IShellObservableFragment IShellContext.CreateFragmentForPage(Page page)
		{
			return CreateFragmentForPage(page);
		}

		IShellFlyoutContentView IShellContext.CreateShellFlyoutContentView()
		{
			return CreateShellFlyoutContentView();
		}

		IShellItemView IShellContext.CreateShellItemView(ShellItem shellItem)
		{
			return CreateShellItemView(shellItem);
		}

		IShellSectionView IShellContext.CreateShellSectionView(ShellSection shellSection)
		{
			return CreateShellSectionView(shellSection);
		}

		IShellToolbarTracker IShellContext.CreateTrackerForToolbar(Toolbar toolbar)
		{
			return CreateTrackerForToolbar(toolbar);
		}

		IShellToolbarAppearanceTracker IShellContext.CreateToolbarAppearanceTracker()
		{
			return CreateToolbarAppearanceTracker();
		}

		IShellTabLayoutAppearanceTracker IShellContext.CreateTabLayoutAppearanceTracker(ShellSection shellSection)
		{
			return CreateTabLayoutAppearanceTracker(shellSection);
		}

		IShellBottomNavViewAppearanceTracker IShellContext.CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
		{
			return CreateBottomNavViewAppearanceTracker(shellItem);
		}

		#endregion IShellContext

		#region IAppearanceObserver

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			UpdateStatusBarColor(appearance);
		}

		#endregion IAppearanceObserver


		// These are the primary colors in our styles.xml file
		public static Color DefaultBackgroundColor => ResolveThemeColor(Color.FromArgb("#2c3e50"), Color.FromArgb("#1B3147"));

		public static readonly Color DefaultForegroundColor = Colors.White;
		public static readonly Color DefaultTitleColor = Colors.White;
		public static readonly Color DefaultUnselectedColor = Color.FromRgba(255, 255, 255, 180);
		internal static Color DefaultBottomNavigationViewBackgroundColor => ResolveThemeColor(Colors.White, Color.FromArgb("#1B3147"));

		internal static bool IsDarkTheme => (Application.Current?.RequestedTheme == OSAppTheme.Dark);

		static Color ResolveThemeColor(Color light, Color dark)
		{
			if (IsDarkTheme)
			{
				return dark;
			}

			return light;
		}

		//bool _disposed;
		IShellFlyoutView _flyoutView;
		FrameLayout _frameLayout;

		//event EventHandler<VisualElementChangedEventArgs> _elementChanged;
		event EventHandler<PropertyChangedEventArgs> _elementPropertyChanged;

		public ShellView(Context context)
		{
			AndroidContext = context;
		}



		protected Context AndroidContext { get; }
		protected Shell Element { get; private set; }
		FragmentManager FragmentManager => AndroidContext.GetFragmentManager();

		protected virtual IShellObservableFragment CreateFragmentForPage(Page page)
		{
			return new ShellContentFragment(this, page);
		}

		protected virtual IShellFlyoutContentView CreateShellFlyoutContentView()
		{
			return new ShellFlyoutTemplatedContentView(this);
			//return new ShellFlyoutContentView(this, AndroidContext);
		}

		protected virtual IShellFlyoutView CreateShellFlyoutView()
		{
			return new ShellFlyoutView(this, AndroidContext);
		}

		protected virtual IShellItemView CreateShellItemView(ShellItem shellItem)
		{
			return new ShellItemView(this);
		}

		protected virtual IShellSectionView CreateShellSectionView(ShellSection shellSection)
		{
			return new ShellSectionView(this);
		}

		protected virtual IShellToolbarTracker CreateTrackerForToolbar(Toolbar toolbar)
		{
			return new ShellToolbarTracker(this, toolbar, ((IShellContext)this).CurrentDrawerLayout);
		}

		protected virtual IShellToolbarAppearanceTracker CreateToolbarAppearanceTracker()
		{
			return new ShellToolbarAppearanceTracker(this);
		}

		protected virtual IShellTabLayoutAppearanceTracker CreateTabLayoutAppearanceTracker(ShellSection shellSection)
		{
			return new ShellTabLayoutAppearanceTracker(this);
		}

		protected virtual IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
		{
			return new ShellBottomNavViewAppearanceTracker(this, shellItem);
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Profile.FrameBegin();

			if (e.PropertyName == Shell.CurrentItemProperty.PropertyName)
				SwitchFragment(FragmentManager, _frameLayout, Element.CurrentItem);

			_elementPropertyChanged?.Invoke(sender, e);

			Profile.FrameEnd();
		}

		internal void SetVirtualView(Shell shell)
		{
			Element = shell;
			shell.SizeChanged += OnElementSizeChanged;
			OnElementSet(shell);
			shell.PropertyChanged += OnElementPropertyChanged;
		}

		protected virtual void OnElementSet(Shell shell)
		{
			Element = shell;
			Profile.FrameBegin();

			Profile.FramePartition("Flyout");
			_flyoutView = CreateShellFlyoutView();

			Profile.FramePartition("Frame");
			_frameLayout = new CustomFrameLayout(AndroidContext)
			{
				LayoutParameters = new LP(LP.MatchParent, LP.MatchParent),
				Id = AView.GenerateViewId(),
			};

			Profile.FramePartition("SetFitsSystemWindows");
			_frameLayout.SetFitsSystemWindows(true);

			Profile.FramePartition("AttachFlyout");
			_flyoutView.AttachFlyout(this, _frameLayout);

			Profile.FramePartition("AddAppearanceObserver");
			((IShellController)shell).AddAppearanceObserver(this, shell);

			// Previewer Hack
			Profile.FramePartition("Previewer Hack");
			if (AndroidContext.GetActivity() != null && shell.CurrentItem != null)
				SwitchFragment(FragmentManager, _frameLayout, shell.CurrentItem, false);

			Profile.FrameEnd();
		}

		IShellItemView _currentView;

		protected virtual void SwitchFragment(FragmentManager manager, AView targetView, ShellItem newItem, bool animate = true)
		{
			Profile.FrameBegin();

			Profile.FramePartition("CreateShellItemView");
			var previousView = _currentView;
			_currentView = CreateShellItemView(newItem);
			_currentView.ShellItem = newItem;
			var fragment = _currentView.Fragment;

			Profile.FramePartition("Transaction");
			FragmentTransaction transaction = manager.BeginTransactionEx();

			if (animate)
				transaction.SetTransitionEx((int)global::Android.App.FragmentTransit.FragmentOpen);

			transaction.ReplaceEx(_frameLayout.Id, fragment);
			transaction.CommitAllowingStateLossEx();

			Profile.FramePartition("OnDestroyed");
			void OnDestroyed(object sender, EventArgs args)
			{
				previousView.Destroyed -= OnDestroyed;

				previousView.Dispose();
				previousView = null;
			}

			if (previousView != null)
				previousView.Destroyed += OnDestroyed;

			Profile.FrameEnd();
		}

		void OnElementSizeChanged(object sender, EventArgs e)
		{
			Profile.FrameBegin();

			Profile.FramePartition("ToPixels");
			int width = (int)AndroidContext.ToPixels(Element.Width);
			int height = (int)AndroidContext.ToPixels(Element.Height);

			Profile.FramePartition("Measure");
			_flyoutView.AndroidView.Measure(MakeMeasureSpec(width, MeasureSpecMode.Exactly),
				MakeMeasureSpec(height, MeasureSpecMode.Exactly));

			Profile.FramePartition("Layout");
			_flyoutView.AndroidView.Layout(0, 0, width, height);

			Profile.FrameEnd();
		}

		int MakeMeasureSpec(int size, MeasureSpecMode mode)
		{
			return size + (int)mode;
		}

		void UpdateStatusBarColor(ShellAppearance appearance)
		{
			Profile.FrameBegin("UpdtStatBarClr");

			var activity = AndroidContext.GetActivity();
			var window = activity?.Window;
			var decorView = window?.DecorView;
			var resources = AndroidContext.Resources;

			int statusBarHeight = 0;
			int resourceId = resources.GetIdentifier("status_bar_height", "dimen", "android");
			if (resourceId > 0)
			{
				statusBarHeight = resources.GetDimensionPixelSize(resourceId);
			}

			int navigationBarHeight = 0;
			resourceId = resources.GetIdentifier("navigation_bar_height", "dimen", "android");
			if (resourceId > 0)
			{
				navigationBarHeight = resources.GetDimensionPixelSize(resourceId);
			}

			// TODO Previewer Hack
			if (decorView != null)
			{
				// we are using the split drawable here to avoid GPU overdraw.
				// All it really is is a drawable that only draws under the statusbar/bottom bar to make sure
				// we dont draw over areas we dont need to. This has very limited benefits considering its
				// only saving us a flat color fill BUT it helps people not freak out about overdraw.
				AColor color;
				if (appearance != null)
				{
					color = appearance.BackgroundColor.ToNative(Color.FromArgb("#03A9F4"));
				}
				else
				{
					color = Color.FromArgb("#03A9F4").ToNative();
				}

				if (!(decorView.Background is SplitDrawable splitDrawable) ||
					splitDrawable.Color != color || splitDrawable.TopSize != statusBarHeight || splitDrawable.BottomSize != navigationBarHeight)
				{
					Profile.FramePartition("Create SplitDrawable");
					var split = new SplitDrawable(color, statusBarHeight, navigationBarHeight);
					Profile.FramePartition("SetBackground");
					decorView.SetBackground(split);
				}
			}

			Profile.FrameEnd("UpdtStatBarClr");
		}

		class SplitDrawable : Drawable
		{
			public int BottomSize { get; }
			public AColor Color { get; }
			public int TopSize { get; }

			public SplitDrawable(AColor color, int topSize, int bottomSize)
			{
				Color = color;
				BottomSize = bottomSize;
				TopSize = topSize;
			}

			public override int Opacity => (int)Format.Opaque;

			public override void Draw(Canvas canvas)
			{
				var bounds = Bounds;

				using (var paint = new Paint())
				{

					paint.Color = Color;

					canvas.DrawRect(new ARect(0, 0, bounds.Right, TopSize), paint);

					canvas.DrawRect(new ARect(0, bounds.Bottom - BottomSize, bounds.Right, bounds.Bottom), paint);

					paint.Dispose();
				}
			}

			public override void SetAlpha(int alpha)
			{
			}

			public override void SetColorFilter(ColorFilter colorFilter)
			{
			}
		}

	}
}