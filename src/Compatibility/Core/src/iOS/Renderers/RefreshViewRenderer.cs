using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class RefreshViewRenderer : ViewRenderer<RefreshView, UIView>, IEffectControlProvider
	{
		bool _isDisposed;
		bool _isRefreshing;
		bool _usingLargeTitles;
		nfloat _originalY;
		nfloat _refreshControlHeight;
		UIView _refreshControlParent;
		UIRefreshControl _refreshControl;

		public bool IsRefreshing
		{
			get { return _isRefreshing; }
			set
			{
				_isRefreshing = value;

				if (Element != null && Element.IsRefreshing != _isRefreshing)
					Element.SetValueFromRenderer(RefreshView.IsRefreshingProperty, _isRefreshing);

				if (_isRefreshing != _refreshControl.Refreshing)
				{
					if (_isRefreshing)
					{
						TryOffsetRefresh(this, IsRefreshing);
						_refreshControl.BeginRefreshing();
					}
					else
					{
						_refreshControl.EndRefreshing();
						TryOffsetRefresh(this, IsRefreshing);
					}
				}
			}
		}

		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public RefreshViewRenderer()
		{

		}

		protected override void OnElementChanged(ElementChangedEventArgs<RefreshView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null || Element == null)
				return;

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					if (Forms.IsiOS11OrNewer)
					{
						var parentNav = e.NewElement.FindParentOfType<NavigationPage>();
						_usingLargeTitles = parentNav != null && parentNav.OnThisPlatform().PrefersLargeTitles();
					}

					_refreshControl = new UIRefreshControl();
					_refreshControl.ValueChanged += OnRefresh;
					_refreshControlParent = this;
				}
			}

			UpdateColors();
			UpdateIsEnabled();
			UpdateIsRefreshing();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateIsEnabled();
			else if (e.PropertyName == RefreshView.IsRefreshingProperty.PropertyName)
				UpdateIsRefreshing();
			else if (e.IsOneOf(RefreshView.RefreshColorProperty, VisualElement.BackgroundColorProperty))
				UpdateColors();
		}

		protected override void SetBackgroundColor(Color color)
		{
			if (_refreshControl == null)
				return;

			_refreshControl.BackgroundColor = color?.ToUIColor();
		}

		protected override void SetBackground(Brush brush)
		{
			if (_refreshControl == null)
				return;

			_refreshControl.UpdateBackground(brush);
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing && Control != null)
			{
				_refreshControl.ValueChanged -= OnRefresh;
				_refreshControl.Dispose();
				_refreshControl = null;
				_refreshControlParent = null;
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}

		bool TryOffsetRefresh(UIView view, bool refreshing)
		{
			if (view is UIScrollView scrollView)
			{
				if (scrollView.ContentOffset.Y < 0)
					return true;

				if (refreshing)
					scrollView.SetContentOffset(new CoreGraphics.CGPoint(0, _originalY - _refreshControlHeight), true);
				else
					scrollView.SetContentOffset(new CoreGraphics.CGPoint(0, _originalY), true);

				return true;
			}

			if (view is WkWebViewRenderer)
			{
				return true;
			}

			if (view.Subviews == null)
				return false;

			for (int i = 0; i < view.Subviews.Length; i++)
			{
				var control = view.Subviews[i];
				if (TryOffsetRefresh(control, refreshing))
					return true;
			}

			return false;
		}

		bool TryRemoveRefresh(UIView view, int index = 0)
		{
			_refreshControlParent = view;

			if (_refreshControl.Superview != null)
				_refreshControl.RemoveFromSuperview();

			if (view is UIScrollView scrollView)
			{
				if (CanUseRefreshControlProperty())
					scrollView.RefreshControl = null;

				return true;
			}

			if (view.Subviews == null)
				return false;

			for (int i = 0; i < view.Subviews.Length; i++)
			{
				var control = view.Subviews[i];
				if (TryRemoveRefresh(control, i))
					return true;
			}

			return false;
		}

		bool TryInsertRefresh(UIView view, int index = 0)
		{
			_refreshControlParent = view;

			if (view is UIScrollView scrollView)
			{
				if (CanUseRefreshControlProperty())
					scrollView.RefreshControl = _refreshControl;
				else
					scrollView.InsertSubview(_refreshControl, index);

				scrollView.AlwaysBounceVertical = true;

				_originalY = scrollView.ContentOffset.Y;
				_refreshControlHeight = _refreshControl.Frame.Size.Height;

				return true;
			}

			if (view is WkWebViewRenderer webView)
			{
				webView.ScrollView.InsertSubview(_refreshControl, index);
				return true;
			}

			if (view.Subviews == null)
				return false;

			for (int i = 0; i < view.Subviews.Length; i++)
			{
				var control = view.Subviews[i];
				if (TryInsertRefresh(control, i))
					return true;
			}

			return false;
		}

		void UpdateColors()
		{
			if (Element == null || _refreshControl == null)
				return;

			if (Element.RefreshColor != null)
				_refreshControl.TintColor = Element.RefreshColor.ToUIColor();

			SetBackgroundColor(Element.BackgroundColor);
			SetBackground(Element.Background);
		}

		void UpdateIsRefreshing()
		{
			IsRefreshing = Element.IsRefreshing;
		}

		void UpdateIsEnabled()
		{
			bool isRefreshViewEnabled = Element.IsEnabled;
			_refreshControl.Enabled = isRefreshViewEnabled;

			UserInteractionEnabled = true;

			if (IsRefreshing)
				return;

			if (isRefreshViewEnabled)
				TryInsertRefresh(_refreshControlParent);
			else
				TryRemoveRefresh(_refreshControlParent);

			UserInteractionEnabled = true;
		}

		bool CanUseRefreshControlProperty()
		{
			return Forms.IsiOS10OrNewer && !_usingLargeTitles;
		}

		void OnRefresh(object sender, EventArgs e)
		{
			IsRefreshing = true;
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			VisualElementRenderer<VisualElement>.RegisterEffect(effect, this, NativeView);
		}
	}
}
