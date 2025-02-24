#nullable disable
using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using AndroidX.DrawerLayout.Widget;
using AndroidX.Fragment.App;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;
using ARect = Android.Graphics.Rect;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;
using AView = Android.Views.View;
using Color = Microsoft.Maui.Graphics.Color;
using LP = Android.Views.ViewGroup.LayoutParams;
using Paint = Android.Graphics.Paint;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class ShellRenderer : IShellContext, IAppearanceObserver, IPlatformViewHandler
	{
		public static IPropertyMapper<Shell, ShellRenderer> Mapper = new PropertyMapper<Shell, ShellRenderer>(ViewHandler.ElementMapper);
		public static CommandMapper<Shell, ShellRenderer> CommandMapper = new CommandMapper<Shell, ShellRenderer>(ViewHandler.ElementCommandMapper);


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

		IShellFlyoutContentRenderer IShellContext.CreateShellFlyoutContentRenderer()
		{
			return CreateShellFlyoutContentRenderer();
		}

		IShellItemRenderer IShellContext.CreateShellItemRenderer(ShellItem shellItem)
		{
			return CreateShellItemRenderer(shellItem);
		}

		IShellSectionRenderer IShellContext.CreateShellSectionRenderer(ShellSection shellSection)
		{
			return CreateShellSectionRenderer(shellSection);
		}

		IShellToolbarTracker IShellContext.CreateTrackerForToolbar(AToolbar toolbar)
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
		}

		#endregion IAppearanceObserver


		// These are the primary colors in our styles.xml file
		public static Color DefaultBackgroundColor => ResolveThemeColor(Color.FromArgb("#2c3e50"), Color.FromArgb("#1B3147"));

		public static readonly Color DefaultForegroundColor = Colors.White;
		public static readonly Color DefaultTitleColor = Colors.White;
		public static readonly Color DefaultUnselectedColor = Color.FromRgba(255, 255, 255, 180);
		internal static Color DefaultBottomNavigationViewBackgroundColor => ResolveThemeColor(Colors.White, Color.FromArgb("#1B3147"));

		internal static bool IsDarkTheme => (Application.Current?.RequestedTheme == AppTheme.Dark);

		static Color ResolveThemeColor(Color light, Color dark)
		{
			if (IsDarkTheme)
			{
				return dark;
			}

			return light;
		}

		IShellFlyoutRenderer _flyoutView;
		FrameLayout _frameLayout;
		IMauiContext _mauiContext;
		bool _disposed;

		event EventHandler<PropertyChangedEventArgs> _elementPropertyChanged;

		public ShellRenderer()
		{

		}

		public ShellRenderer(Context context)
		{
			AndroidContext = context;
		}



		protected Context AndroidContext { get; private set; }
		protected Shell Element { get; private set; }
		FragmentManager FragmentManager =>
			Element.FindMauiContext().GetFragmentManager();

		protected virtual IShellObservableFragment CreateFragmentForPage(Page page)
		{
			return new ShellContentFragment(this, page);
		}

		protected virtual IShellFlyoutContentRenderer CreateShellFlyoutContentRenderer()
		{
			return new ShellFlyoutTemplatedContentRenderer(this);
		}

		protected virtual IShellFlyoutRenderer CreateShellFlyoutRenderer()
		{
			return new ShellFlyoutRenderer(this, AndroidContext);
		}

		protected virtual IShellItemRenderer CreateShellItemRenderer(ShellItem shellItem)
		{
			return new ShellItemRenderer(this);
		}

		protected virtual IShellSectionRenderer CreateShellSectionRenderer(ShellSection shellSection)
		{
			return new ShellSectionRenderer(this);
		}

		protected virtual IShellToolbarTracker CreateTrackerForToolbar(AToolbar toolbar)
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
			if (e.PropertyName == Shell.CurrentItemProperty.PropertyName)
				SwitchFragment(FragmentManager, _frameLayout, Element.CurrentItem, Element.IsAnimateOnNavigation);

			_elementPropertyChanged?.Invoke(sender, e);
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

			_flyoutView = CreateShellFlyoutRenderer();
			_frameLayout = new CustomFrameLayout(AndroidContext)
			{
				LayoutParameters = new LP(LP.MatchParent, LP.MatchParent),
				Id = AView.GenerateViewId(),
			};

			_flyoutView.AttachFlyout(this, _frameLayout);

			((IShellController)shell).AddAppearanceObserver(this, shell);

			SwitchFragment(FragmentManager, _frameLayout, shell.CurrentItem, false);
		}

		IShellItemRenderer _currentView;

		protected virtual void SwitchFragment(FragmentManager manager, AView targetView, ShellItem newItem, bool animate = true)
		{
			var previousView = _currentView;
			_currentView = CreateShellItemRenderer(newItem);
			_currentView.ShellItem = newItem;
			var fragment = _currentView.Fragment;

			FragmentTransaction transaction = manager.BeginTransactionEx();

			if (animate)
			{
				transaction.SetCustomAnimations(global::Android.Resource.Animation.FadeIn, global::Android.Resource.Animation.FadeOut);
			}

			transaction.ReplaceEx(_frameLayout.Id, fragment);

			// Don't force the commit if this is our first load 
			if (previousView == null)
			{
				transaction.SetReorderingAllowedEx(true);
			}

			transaction.CommitAllowingStateLossEx();

			void OnDestroyed(object sender, EventArgs args)
			{
				previousView.Destroyed -= OnDestroyed;

				previousView = null;
			}

			if (previousView != null)
				previousView.Destroyed += OnDestroyed;
		}

		void OnElementSizeChanged(object sender, EventArgs e)
		{
			int width = (int)AndroidContext.ToPixels(Element.Width);
			int height = (int)AndroidContext.ToPixels(Element.Height);

			_flyoutView.AndroidView.Measure(MakeMeasureSpec(width, MeasureSpecMode.Exactly),
				MakeMeasureSpec(height, MeasureSpecMode.Exactly));

			_flyoutView.AndroidView.Layout(0, 0, width, height);
		}

		int MakeMeasureSpec(int size, MeasureSpecMode mode)
		{
			return size + (int)mode;
		}

		bool IViewHandler.HasContainer { get => false; set { } }

		object IViewHandler.ContainerView => null;

		IView IViewHandler.VirtualView => Element;

		object IElementHandler.PlatformView => _flyoutView.AndroidView;

		Maui.IElement IElementHandler.VirtualView => Element;

		IMauiContext IElementHandler.MauiContext => _mauiContext;

		AView IPlatformViewHandler.PlatformView => _flyoutView.AndroidView;

		AView IPlatformViewHandler.ContainerView => null;

		Size IViewHandler.GetDesiredSize(double widthConstraint, double heightConstraint) => new Size(100, 100);

		void IViewHandler.PlatformArrange(Graphics.Rect rect)
		{
			//TODO I don't think we need this
		}

		void IElementHandler.SetMauiContext(IMauiContext mauiContext)
		{
			_mauiContext = mauiContext;
			AndroidContext = mauiContext.Context;
		}

		void IElementHandler.SetVirtualView(Maui.IElement view)
		{
			SetVirtualView((Shell)view);
		}

		void IElementHandler.UpdateValue(string property)
		{
			Mapper.UpdateProperty(this, Element, property);
		}

		void IElementHandler.Invoke(string command, object args)
		{
			CommandMapper.Invoke(this, Element, command, args);
		}

		void IElementHandler.DisconnectHandler()
		{
			if (_disposed)
				return;

			_disposed = true;

			Element.PropertyChanged -= OnElementPropertyChanged;
			Element.SizeChanged -= OnElementSizeChanged;
			((IShellController)Element).RemoveAppearanceObserver(this);

			if (_flyoutView is ShellFlyoutRenderer sfr)
				sfr.Disconnect();
			else
				(_flyoutView as IDisposable)?.Dispose();

			if (_currentView is ShellItemRendererBase sir)
				sir.Disconnect();
			else
				_currentView.Dispose();

			_currentView = null;

			Element = null;

			_disposed = true;
		}
	}
}
