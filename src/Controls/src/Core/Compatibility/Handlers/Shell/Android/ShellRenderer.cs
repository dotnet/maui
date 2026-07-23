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
using Microsoft.Maui.Platform;
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
		public static Color DefaultBackgroundColor => ResolveThemeColor(RuntimeFeature.IsMaterial3Enabled ? Color.FromArgb("#FEF7FF") : Color.FromArgb("#2c3e50"), RuntimeFeature.IsMaterial3Enabled ? Color.FromArgb("#141218") : Color.FromArgb("#1B3147"));
		public static Color DefaultForegroundColor => ResolveThemeColor(RuntimeFeature.IsMaterial3Enabled ? Color.FromArgb("#1D1B20") : Colors.Black, RuntimeFeature.IsMaterial3Enabled ? Color.FromArgb("#E6E0E9") : Colors.White);
		public static Color DefaultTitleColor => ResolveThemeColor(RuntimeFeature.IsMaterial3Enabled ? Color.FromArgb("#1D1B20") : Colors.White, RuntimeFeature.IsMaterial3Enabled ? Color.FromArgb("#E6E0E9") : Colors.White);
		public static Color DefaultUnselectedColor => ResolveThemeColor(
			RuntimeFeature.IsMaterial3Enabled ? Color.FromArgb("#49454F") : Color.FromRgba(255, 255, 255, 180),
			RuntimeFeature.IsMaterial3Enabled ? Color.FromArgb("#CAC4D0") : Color.FromRgba(255, 255, 255, 180));
		internal static Color DefaultBottomNavigationViewBackgroundColor => ResolveThemeColor(RuntimeFeature.IsMaterial3Enabled ? Color.FromArgb("#F3EDF7") : Colors.White, RuntimeFeature.IsMaterial3Enabled ? Color.FromArgb("#1D1B20") : Color.FromArgb("#1B3147"));
		internal static bool IsDarkTheme => Application.Current?.RequestedTheme == AppTheme.Dark;

		static Color ResolveThemeColor(Color light, Color dark)
		{
			if (IsDarkTheme)
			{
				return dark;
			}

			return light;
		}

		// Material 3 defines these color roles as theme attributes on Theme.Material3.DayNight,
		// so reading them straight from the Context automatically tracks light/dark and any
		// app-level M3 theme customization instead of duplicating hardcoded hex values here.
		internal static Color GetM3BackgroundColor(Context context) =>
			new AColor(context.GetThemeAttrColor(Resource.Attribute.colorSurface)).ToColor();
		internal static Color GetM3ForegroundColor(Context context) =>
			new AColor(context.GetThemeAttrColor(Resource.Attribute.colorPrimary)).ToColor();
		internal static Color GetM3TitleColor(Context context) =>
			new AColor(context.GetThemeAttrColor(Resource.Attribute.colorOnSurface)).ToColor();
		internal static Color GetM3UnselectedColor(Context context) =>
			new AColor(context.GetThemeAttrColor(Resource.Attribute.colorOnSurfaceVariant)).ToColor();
		internal static Color GetM3BottomNavBackgroundColor(Context context) =>
			new AColor(context.GetThemeAttrColor(Resource.Attribute.colorSurface)).ToColor();

		// Material 2 — MAUI's Maui.MainTheme.Base already declares colorPrimary/colorPrimaryDark/
		// actionMenuTextColor with the same values previously hardcoded here. Reading them through
		// the theme means an app that overrides <color name="colorPrimary"/> in its colors.xml
		// gets a consistent answer from both the native drawables and the C# ResetAppearance path.
		// android:textColorPrimary and android:colorBackground come from the platform DayNight
		// parent so they swap correctly in dark mode without extra branching.
		internal static Color GetM2BackgroundColor(Context context) =>
			IsDarkTheme
				? new AColor(context.GetThemeAttrColor(Resource.Attribute.colorPrimaryDark)).ToColor()
				: new AColor(context.GetThemeAttrColor(Resource.Attribute.colorPrimary)).ToColor();
		internal static Color GetM2ForegroundColor(Context context) =>
			new AColor(context.GetThemeAttrColor(global::Android.Resource.Attribute.TextColorPrimary)).ToColor();
		internal static Color GetM2TitleColor(Context context) =>
			new AColor(context.GetThemeAttrColor(Resource.Attribute.actionMenuTextColor)).ToColor();
		internal static Color GetM2UnselectedColor(Context context) =>
			GetM2TitleColor(context).MultiplyAlpha(180f / 255f);
		internal static Color GetM2BottomNavBackgroundColor(Context context) =>
			IsDarkTheme
				? new AColor(context.GetThemeAttrColor(Resource.Attribute.colorPrimaryDark)).ToColor()
				: new AColor(context.GetThemeAttrColor(global::Android.Resource.Attribute.ColorBackground)).ToColor();

		// Context-aware accessors used by the appearance trackers: resolve from the M3 theme
		// attributes above when Material 3 is enabled, otherwise resolve from the M2 attributes
		// (declared on MAUI's default Android theme). Either way the value comes from the
		// Android theme, not a hardcoded hex, so there is only ever one source of truth.
		internal static Color GetBackgroundColor(Context context) =>
			RuntimeFeature.IsMaterial3Enabled ? GetM3BackgroundColor(context) : GetM2BackgroundColor(context);
		internal static Color GetForegroundColor(Context context) =>
			RuntimeFeature.IsMaterial3Enabled ? GetM3ForegroundColor(context) : GetM2ForegroundColor(context);
		internal static Color GetTitleColor(Context context) =>
			RuntimeFeature.IsMaterial3Enabled ? GetM3TitleColor(context) : GetM2TitleColor(context);
		internal static Color GetUnselectedColor(Context context) =>
			RuntimeFeature.IsMaterial3Enabled ? GetM3UnselectedColor(context) : GetM2UnselectedColor(context);
		internal static Color GetBottomNavigationViewBackgroundColor(Context context) =>
			RuntimeFeature.IsMaterial3Enabled ? GetM3BottomNavBackgroundColor(context) : GetM2BottomNavBackgroundColor(context);

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
				SwitchFragment(FragmentManager, _frameLayout, Element.CurrentItem);

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
				transaction.SetTransitionEx((int)global::Android.App.FragmentTransit.FragmentOpen);

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
