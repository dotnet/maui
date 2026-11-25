#nullable disable
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.Core.Content;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;
using AColorRes = Android.Resource.Color;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	internal class ShellPageContainer : ViewGroup
	{
		readonly ShellPageBackground _background = new ShellPageBackground();
		Application _themeApplication;
		public IViewHandler Child { get; set; }

		public bool IsInFragment { get; set; }

		public ShellPageContainer(Context context, IPlatformViewHandler child, bool inFragment = false) : base(context)
		{
			Child = child;
			IsInFragment = inFragment;
			if (child.VirtualView.Background is null)
			{
				_background.Initialize(child.PlatformView, context, Application.Current?.RequestedTheme ?? AppTheme.Unspecified);
			}
			child.PlatformView.RemoveFromParent();
			AddView(child.PlatformView);
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();
			if (Application.Current is { } application)
			{
				_themeApplication = application;
				_themeApplication.RequestedThemeChanged += OnRequestedThemeChanged;
				UpdateDefaultBackground(application.RequestedTheme);
			}
		}

		protected override void OnDetachedFromWindow()
		{
			if (_themeApplication is not null)
				_themeApplication.RequestedThemeChanged -= OnRequestedThemeChanged;
			_themeApplication = null;
			base.OnDetachedFromWindow();
		}

		void OnRequestedThemeChanged(object sender, AppThemeChangedEventArgs args)
		{
			if (sender is not Application application)
				return;

			application.Dispatcher.DispatchIfRequired(() =>
			{
				if (ReferenceEquals(_themeApplication, application) && application.RequestedTheme == args.RequestedTheme)
					UpdateDefaultBackground(args.RequestedTheme);
			});
		}

		void UpdateDefaultBackground(AppTheme theme)
		{
			if (Child?.VirtualView is { Background: null } && Child.PlatformView is AView platformView)
				_background.Update(platformView, Context, theme);
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			var width = r - l;
			var height = b - t;

			if (Child.PlatformView is AView aView)
				aView.Layout(0, 0, width, height);
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (Child.PlatformView is AView aView)
			{
				aView.Measure(widthMeasureSpec, heightMeasureSpec);
				SetMeasuredDimension(aView.MeasuredWidth, aView.MeasuredHeight);
			}
			else
				SetMeasuredDimension(0, 0);
		}
	}

	internal sealed class ShellPageBackground
	{
		Drawable _background;
		int _color;

		public void Initialize(AView view, Context context, AppTheme theme) => SetBackground(view, context, theme);

		public void Update(AView view, Context context, AppTheme theme)
		{
			var currentBackground = view.Background;
			if (_background is null)
			{
				if (currentBackground is not null)
					return;
			}
			else if (!ReferenceEquals(currentBackground, _background) ||
				currentBackground is not ColorDrawable colorDrawable || colorDrawable.Color.ToArgb() != _color)
			{
				return;
			}

			SetBackground(view, context, theme);
		}

		void SetBackground(AView view, Context context, AppTheme theme)
		{
			if (RuntimeFeature.IsMaterial3Enabled)
			{
				using var themedContext = context.CreateThemeContext(theme);
				_color = ContextExtensions.GetThemeAttrColor(themedContext, Resource.Attribute.colorSurface);
			}
			else
			{
				_color = ContextCompat.GetColor(context, theme == AppTheme.Dark ? AColorRes.BackgroundDark : AColorRes.BackgroundLight);
			}

			view.SetBackgroundColor(new AColor(_color));
			_background = view.Background;
		}
	}
}
