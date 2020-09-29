using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using AndroidX.DrawerLayout.Widget;
using Xamarin.Forms.Internals;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellFlyoutRenderer : DrawerLayout, IShellFlyoutRenderer, DrawerLayout.IDrawerListener, IFlyoutBehaviorObserver, IAppearanceObserver
	{
		#region IAppearanceObserver

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			if (appearance == null)
				UpdateScrim(Brush.Transparent);
			else
			{
				UpdateScrim(appearance.FlyoutBackdrop);
			}
		}
		#endregion IAppearanceObserver

		#region IShellFlyoutRenderer

		AView IShellFlyoutRenderer.AndroidView => this;

		void IShellFlyoutRenderer.AttachFlyout(IShellContext context, AView content)
		{
			AttachFlyout(context, content);
		}

		#endregion IShellFlyoutRenderer

		#region IDrawerListener

		void IDrawerListener.OnDrawerClosed(AView drawerView)
		{
			Shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, false);
		}

		void IDrawerListener.OnDrawerOpened(AView drawerView)
		{
			Shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, true);
		}

		void IDrawerListener.OnDrawerSlide(AView drawerView, float slideOffset)
		{
			_scrimOpacity = (int)(slideOffset * 255);
		}

		void IDrawerListener.OnDrawerStateChanged(int newState)
		{
			if (DrawerLayout.StateIdle == newState)
			{
				Shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, IsDrawerOpen(_flyoutContent.AndroidView));
			}
		}

		#endregion IDrawerListener

		#region IFlyoutBehaviorObserver

		void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
			_behavior = behavior;
			UpdateDrawerLockMode(behavior);
		}

		#endregion IFlyoutBehaviorObserver

		const uint DefaultScrimColor = 0x99000000;
		readonly IShellContext _shellContext;
		AView _content;
		IShellFlyoutContentRenderer _flyoutContent;
		int _flyoutWidth;
		int _currentLockMode;
		bool _disposed;
		Brush _scrimBrush;
		Paint _scrimPaint;
		int _previousHeight;
		int _previousWidth;
		int _scrimOpacity;

		FlyoutBehavior _behavior;

		public ShellFlyoutRenderer(IShellContext shellContext, Context context) : base(context)
		{
			_scrimBrush = Brush.Default;
			_shellContext = shellContext;

			Shell.PropertyChanged += OnShellPropertyChanged;

			ShellController.AddAppearanceObserver(this, Shell);
		}

		Shell Shell => _shellContext.Shell;

		IShellController ShellController => _shellContext.Shell;

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			bool result = base.OnInterceptTouchEvent(ev);

			if (GetDrawerLockMode(_flyoutContent.AndroidView) == LockModeLockedOpen)
				return false;

			return result;
		}

		protected override bool DrawChild(Canvas canvas, AView child, long drawingTime)
		{
			bool returnValue = base.DrawChild(canvas, child, drawingTime);
			if (_scrimPaint != null && ((LayoutParams)child.LayoutParameters).Gravity == (int)GravityFlags.NoGravity)
			{
				if (_previousHeight != Height || _previousWidth != Width)
				{
					_scrimPaint.UpdateBackground(_scrimBrush, Height, Width);
					_previousHeight = Height;
					_previousWidth = Width;
				}

				_scrimPaint.Alpha = _scrimOpacity;
				canvas.DrawRect(0, 0, Width, Height, _scrimPaint);
			}

			return returnValue;
		}

		protected virtual void AttachFlyout(IShellContext context, AView content)
		{
			Profile.FrameBegin();

			_content = content;

			Profile.FramePartition("Create ContentRenderer");
			_flyoutContent = context.CreateShellFlyoutContentRenderer();

			// Depending on what you read the right edge of the drawer should be Max(56dp, actionBarSize)
			// from the right edge of the screen. Fine. Well except that doesn't account
			// for landscape devices, in which case its still, according to design
			// documents from google 56dp, except google doesn't do that with their own apps.
			// So we are just going to go ahead and do what google does here even though
			// this isn't what DrawerLayout does by default.

			// Oh then there is this rule about how wide it should be at most. It should not
			// at least according to docs be more than 6 * actionBarSize wide. Again non of
			// this is about landscape devices and google does not perfectly follow these
			// rules... so we'll kind of just... do our best.

			Profile.FramePartition("Fudge Width");
			var metrics = Context.Resources.DisplayMetrics;
			var width = Math.Min(metrics.WidthPixels, metrics.HeightPixels);

			var actionBarHeight = (int)Context.ToPixels(56);
			using (var tv = new TypedValue())
			{
				if (Context.Theme.ResolveAttribute(global::Android.Resource.Attribute.ActionBarSize, tv, true))
				{
					actionBarHeight = TypedValue.ComplexToDimensionPixelSize(tv.Data, metrics);
				}
			}

			width -= actionBarHeight;

			var maxWidth = actionBarHeight * 6;
			width = Math.Min(width, maxWidth);

			_flyoutWidth = width;

			_flyoutContent.AndroidView.LayoutParameters =
				new LayoutParams(width, LP.MatchParent) { Gravity = (int)GravityFlags.Start };

			Profile.FramePartition("AddView Content");
			AddView(content);

			Profile.FramePartition("AddView Flyout");
			AddView(_flyoutContent.AndroidView);

			Profile.FramePartition("Add DrawerListener");
			AddDrawerListener(this);

			Profile.FramePartition("Add BehaviorObserver");
			((IShellController)context.Shell).AddFlyoutBehaviorObserver(this);

			Profile.FrameEnd();

			if (Shell.FlyoutIsPresented)
				OpenDrawer(_flyoutContent.AndroidView, false);
		}

		protected virtual void OnShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.FlyoutIsPresentedProperty.PropertyName)
			{
				var presented = Shell.FlyoutIsPresented;
				if (presented)
					OpenDrawer(_flyoutContent.AndroidView, true);
				else
					CloseDrawers();
			}
		}

		protected virtual void UpdateDrawerLockMode(FlyoutBehavior behavior)
		{
			switch (behavior)
			{
				case FlyoutBehavior.Disabled:
					CloseDrawers();
					Shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, false);
					_currentLockMode = LockModeLockedClosed;
					SetDrawerLockMode(_currentLockMode);
					_content.SetPadding(0, _content.PaddingTop, _content.PaddingRight, _content.PaddingBottom);
					break;

				case FlyoutBehavior.Flyout:
					_currentLockMode = LockModeUnlocked;
					SetDrawerLockMode(_currentLockMode);
					_content.SetPadding(0, _content.PaddingTop, _content.PaddingRight, _content.PaddingBottom);
					break;

				case FlyoutBehavior.Locked:
					Shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, true);
					_currentLockMode = LockModeLockedOpen;
					SetDrawerLockMode(_currentLockMode);
					_content.SetPadding(_flyoutWidth, _content.PaddingTop, _content.PaddingRight, _content.PaddingBottom);
					break;
			}

			UpdateScrim(_scrimBrush);
		}

		void UpdateScrim(Brush backdrop)
		{
			_scrimBrush = backdrop;

			if (_behavior == FlyoutBehavior.Locked)
			{
				SetScrimColor(Color.Transparent.ToAndroid());
				_scrimPaint = null;
			}
			else
			{
				if (backdrop is SolidColorBrush solidColor)
				{
					_scrimPaint = null;
					var backdropColor = solidColor.Color;
					if (backdropColor == Color.Default)
					{
						unchecked
						{
							SetScrimColor((int)DefaultScrimColor);
						}
					}
					else
						SetScrimColor(backdropColor.ToAndroid());
				}
				else
				{
					_scrimPaint = _scrimPaint ?? new Paint();
					_scrimPaint.UpdateBackground(_scrimBrush, Height, Width);
					SetScrimColor(Color.Transparent.ToAndroid());
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				ShellController.RemoveAppearanceObserver(this);
				Shell.PropertyChanged -= OnShellPropertyChanged;

				RemoveDrawerListener(this);
				((IShellController)_shellContext.Shell).RemoveFlyoutBehaviorObserver(this);

				RemoveView(_content);
				RemoveView(_flyoutContent.AndroidView);

				_flyoutContent.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}