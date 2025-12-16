#nullable disable
using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using AndroidX.DrawerLayout.Widget;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;
using Color = Microsoft.Maui.Graphics.Color;
using LD = Android.Views.LayoutDirection;
using LP = Android.Views.ViewGroup.LayoutParams;
using Paint = Android.Graphics.Paint;

#pragma warning disable RS0016 // Add public types and members to the declared API
namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellFlyoutRenderer : DrawerLayout, IShellFlyoutRenderer, IFlyoutBehaviorObserver, IAppearanceObserver
	{
		#region IAppearanceObserver


		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			var previousFlyoutWidth = FlyoutWidth;
			var previousFlyoutHeight = FlyoutHeight;

			if (appearance == null)
			{
				UpdateScrim(Brush.Transparent);
				_flyoutWidth = -1;
				_flyoutHeight = LP.MatchParent;
			}
			else
			{
				UpdateScrim(appearance.FlyoutBackdrop);

				if (appearance.FlyoutHeight != -1)
					_flyoutHeight = Context.ToPixels(appearance.FlyoutHeight);
				else
					_flyoutHeight = LP.MatchParent;

				if (appearance.FlyoutWidth != -1)
					_flyoutWidth = Context.ToPixels(appearance.FlyoutWidth);
				else
					_flyoutWidth = -1;
			}

			if (previousFlyoutWidth != FlyoutWidth || previousFlyoutHeight != FlyoutHeight)
			{
				UpdateFlyoutSize();
				if (_content != null)
					UpdateDrawerLockMode(_behavior);
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

		void OnDrawerStateChanged(object sender, DrawerStateChangedEventArgs e)
		{
			if (_flyoutContent?.AndroidView == null)
				return;

			if (DrawerLayout.StateIdle == e.NewState)
			{
				Shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, IsDrawerOpen(_flyoutContent.AndroidView));
			}
		}

		void OnDrawerOpened(object sender, DrawerOpenedEventArgs e)
		{
			Shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, true);
		}

		void OnDrawerSlide(object sender, DrawerSlideEventArgs e)
		{
			var slideOffset = e.SlideOffset;
			SlideOffset = slideOffset;
			_scrimOpacity = (int)(slideOffset * 255);
		}

		void OnDrawerClosed(object sender, DrawerClosedEventArgs e)
		{
			Shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, false);
		}

		#endregion IDrawerListener

		#region IFlyoutBehaviorObserver

		void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
			AddFlyoutContentToLayoutIfNeeded(behavior);

			if (_flyoutContent?.AndroidView == null)
				return;

			bool closeAfterUpdate = (behavior == FlyoutBehavior.Flyout && _behavior == FlyoutBehavior.Locked);
			_behavior = behavior;
			UpdateDrawerLockMode(behavior);

			if (closeAfterUpdate)
				CloseDrawer(_flyoutContent.AndroidView, false);
		}

		#endregion IFlyoutBehaviorObserver

		const uint DefaultScrimColor = 0x99000000;
		IShellContext _shellContext;
		AView _content;
		IShellFlyoutContentRenderer _flyoutContent;
		int _flyoutWidthDefault;
		double _flyoutWidth = -1;
		double _flyoutHeight;
		internal bool FlyoutFirstDrawPassFinished { get; private set; }
		int _currentLockMode;
		bool _disposed;
		Brush _scrimBrush;
		Paint _scrimPaint;
		int _previousHeight;
		int _previousWidth;
		int _scrimOpacity;
		FlyoutBehavior _behavior;
		protected float SlideOffset { get; private set; }

		public ShellFlyoutRenderer(IShellContext shellContext, Context context) : base(context)
		{
			_scrimBrush = Brush.Default;
			_shellContext = shellContext;
			_flyoutHeight = LP.MatchParent;

			ShellController.AddAppearanceObserver(this, Shell);
			UpdateFlowDirection();

			this.DrawerClosed += OnDrawerClosed;
			this.DrawerSlide += OnDrawerSlide;
			this.DrawerOpened += OnDrawerOpened;
			this.DrawerStateChanged += OnDrawerStateChanged;
		}

		double FlyoutWidth => (_flyoutWidth == -1) ? _flyoutWidthDefault : _flyoutWidth;
		int FlyoutHeight => (_flyoutHeight == -1) ? LP.MatchParent : (int)_flyoutHeight;
		Shell Shell => _shellContext.Shell;
		IShellController ShellController => _shellContext.Shell;

		internal IShellFlyoutContentRenderer FlyoutContentRenderer => _flyoutContent;

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			bool result = base.OnInterceptTouchEvent(ev);

			if (_flyoutContent != null && GetDrawerLockMode(_flyoutContent.AndroidView) == LockModeLockedOpen)
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

			if (!FlyoutFirstDrawPassFinished && _flyoutContent is not null)
			{
				// If the AndroidView property which is the DrawerLayout is initialized at this point, the Flyout first draw pass finished.
				if (_flyoutContent?.AndroidView is not null)
					FlyoutFirstDrawPassFinished = true;

				if (this.IsDrawerOpen(_flyoutContent.AndroidView) != _shellContext.Shell.FlyoutIsPresented)
				{
					UpdateDrawerState();
				}
			}

			return returnValue;
		}

		protected virtual void AttachFlyout(IShellContext context, AView content)
		{
			_content = content;

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

			var metrics = context.AndroidContext.Resources.DisplayMetrics;
			var width = Math.Min(metrics.WidthPixels, metrics.HeightPixels);

			var actionBarHeight = (int)context.AndroidContext.GetActionBarHeight();

			width -= actionBarHeight;

			var maxWidth = actionBarHeight * 6;
			width = Math.Min(width, maxWidth);

			_flyoutWidthDefault = width;


			AddView(content);

			if (context.Shell is IFlyoutView view)
				AddFlyoutContentToLayoutIfNeeded(view.FlyoutBehavior);

			((IShellController)context.Shell).AddFlyoutBehaviorObserver(this);


			if (Shell.FlyoutIsPresented && _flyoutContent != null)
			{
				OpenDrawer(_flyoutContent.AndroidView, false);
			}
		}

		void AddFlyoutContentToLayoutIfNeeded(FlyoutBehavior behavior)
		{
			if (behavior == FlyoutBehavior.Disabled)
				return;

			if (_flyoutContent == null && ChildCount > 0)
			{
				_flyoutContent = _shellContext.CreateShellFlyoutContentRenderer();

				UpdateFlyoutSize();

				AddView(_flyoutContent.AndroidView);
			}
		}

		/// <summary>
		/// Updates the flyout presented state. Called by ShellHandler mapper or PropertyChanged.
		/// </summary>

		public void UpdateFlyoutPresented(bool presented)
		{
			if (_flyoutContent == null)
				return;

			if (!FlyoutFirstDrawPassFinished)
			{
				// if the first draw pass hasn't happened yet
				// then calling close/open drawer really confuses drawer layout
				return;
			}

			if (presented)
			{
				if (!IsDrawerOpen(_flyoutContent.AndroidView))
					OpenDrawer(_flyoutContent.AndroidView, true);
			}
			else
			{
				CloseDrawers();
			}
		}

		void UpdateDrawerState()
		{
			UpdateFlyoutPresented(Shell.FlyoutIsPresented);
		}

		/// <summary>
		/// Updates the flow direction. Called by ShellHandler mapper or PropertyChanged.
		/// </summary>
		public void UpdateFlowDirection()
		{
			LayoutDirection = _shellContext.Shell.FlowDirection.ToLayoutDirection();
		}

		/// <summary>
		/// Updates the flyout behavior. Called by ShellHandler mapper.
		/// </summary>
		public void UpdateFlyoutBehavior(FlyoutBehavior behavior)
		{
			AddFlyoutContentToLayoutIfNeeded(behavior);

			if (_flyoutContent?.AndroidView == null)
				return;

			bool closeAfterUpdate = (behavior == FlyoutBehavior.Flyout && _behavior == FlyoutBehavior.Locked);
			_behavior = behavior;
			UpdateDrawerLockMode(behavior);

			if (closeAfterUpdate)
				CloseDrawer(_flyoutContent.AndroidView, false);
		}

		/// <summary>
		/// Updates the flyout size (width and height). Called by ShellHandler mapper.
		/// </summary>
		public void UpdateFlyoutSize(double width, double height)
		{
			var previousFlyoutWidth = FlyoutWidth;
			var previousFlyoutHeight = FlyoutHeight;

			if (width != -1)
				_flyoutWidth = Context.ToPixels(width);
			else
				_flyoutWidth = -1;

			if (height != -1)
				_flyoutHeight = Context.ToPixels(height);
			else
				_flyoutHeight = LP.MatchParent;

			if (previousFlyoutWidth != FlyoutWidth || previousFlyoutHeight != FlyoutHeight)
			{
				UpdateFlyoutSize();
				if (_content != null)
					UpdateDrawerLockMode(_behavior);
			}
		}

		/// <summary>
		/// Updates the flyout backdrop (scrim). Called by ShellHandler mapper.
		/// </summary>
		public void UpdateFlyoutBackdrop(Brush backdrop)
		{
			UpdateScrim(backdrop);
		}

		void OnDualScreenServiceScreenChanged(object sender, EventArgs e)
		{
			UpdateFlyoutSize();
			if (_content != null)
				UpdateDrawerLockMode(_behavior);
		}

		protected virtual void UpdateDrawerLockMode(FlyoutBehavior behavior)
		{
			AddFlyoutContentToLayoutIfNeeded(behavior);

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

					_content.SetPadding((int)FlyoutWidth, _content.PaddingTop, _content.PaddingRight, _content.PaddingBottom);
					break;
			}

			UpdateScrim(_scrimBrush);
		}

		double _previouslyMeasuredFlyoutWidth;
		int _previouslyMeasuredFlyoutHeight;

		void UpdateFlyoutSize()
		{
			if (_flyoutContent?.AndroidView == null)
				return;

			UpdateFlyoutSize(_flyoutContent.AndroidView);

			// This forces a redraw of the flyout
			// without this the flyout will just be empty once you change
			// the width
			if (Shell.FlyoutIsPresented)
				OpenDrawer(_flyoutContent.AndroidView, false);
		}

		protected virtual void UpdateFlyoutSize(AView flyoutView)
		{
			if (flyoutView != null &&
				(_previouslyMeasuredFlyoutWidth != FlyoutWidth || _previouslyMeasuredFlyoutHeight != FlyoutHeight))
			{
				_previouslyMeasuredFlyoutWidth = FlyoutWidth;
				_previouslyMeasuredFlyoutHeight = FlyoutHeight;

				flyoutView.LayoutParameters =
					new LayoutParams((int)FlyoutWidth, FlyoutHeight) { Gravity = (int)GravityFlags.Start };
			}
		}

		void UpdateScrim(Brush backdrop)
		{
			_scrimBrush = backdrop;

			if (_behavior == FlyoutBehavior.Locked)
			{
				SetScrimColor(Colors.Transparent.ToPlatform());
				_scrimPaint = null;
			}
			else
			{
				if (backdrop is SolidColorBrush solidColor)
				{
					_scrimPaint = null;
					var backdropColor = solidColor.Color;
					if (backdropColor == null)
					{
						unchecked
						{
							SetScrimColor((int)DefaultScrimColor);
						}
					}
					else
						SetScrimColor(backdropColor.ToPlatform());
				}
				else
				{
					_scrimPaint = _scrimPaint ?? new Paint();
					_scrimPaint.UpdateBackground(_scrimBrush, Height, Width);
					SetScrimColor(Colors.Transparent.ToPlatform());
				}
			}
		}

		internal void Disconnect()
		{
			ShellController?.RemoveAppearanceObserver(this);

			if (this.IsAlive())
			{
				this.DrawerClosed -= OnDrawerClosed;
				this.DrawerSlide -= OnDrawerSlide;
				this.DrawerOpened -= OnDrawerOpened;
				this.DrawerStateChanged -= OnDrawerStateChanged;
			}

			ShellController?.RemoveFlyoutBehaviorObserver(this);

			if (_flyoutContent is ShellFlyoutTemplatedContentRenderer flyoutTemplatedContentRenderer)
			{
				flyoutTemplatedContentRenderer.Disconnect();
			}

			_shellContext = null;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				Disconnect();

				RemoveView(_content);

				if (_flyoutContent != null)
					RemoveView(_flyoutContent.AndroidView);

				_flyoutContent?.Dispose();
			}

			base.Dispose(disposing);
		}

	}
}
