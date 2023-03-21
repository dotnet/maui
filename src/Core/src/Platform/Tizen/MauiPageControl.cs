using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia.Views;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.NUI;
using NExtents = Tizen.NUI.Extents;
using NLayoutParamPolicies = Tizen.NUI.BaseComponents.LayoutParamPolicies;
using NLinearLayout = Tizen.NUI.LinearLayout;
using NPointStateType = Tizen.NUI.PointStateType;
using NSize = Tizen.NUI.Size;
using NView = Tizen.NUI.BaseComponents.View;
using TSize = Tizen.UIExtensions.Common.Size;

namespace Microsoft.Maui.Platform
{
	public class MauiPageControl : ViewGroup, IMeasurable
	{
		const double DefaultMargin = 4;

		IIndicatorView _indicatorView;
		IPlatformViewHandler? _templatedViewHandler;
		ILayout? _templatedView;
		NView? _contentView;

		List<Indicator> _indicators = new List<Indicator>();
		int _currentPoistion = -1;

		public MauiPageControl(IIndicatorView view)
		{
			_indicatorView = view;
			LayoutUpdated += OnLayoutUpdated;
		}

		bool UseDefaultIndicator { get; set; }

		double IndicatorSizeWithMargin => IndicatorSize + DefaultMargin * 2;

		double IndicatorSize => _indicatorView.IndicatorSize;

		public void ResetIndicators()
		{
			ClearIndicatorView();
			if ((_indicatorView as ITemplatedIndicatorView)?.IndicatorsLayoutOverride == null)
			{
				CreateDefaultView();
			}
			else
			{
				CreateTemplatedView();
			}
			this.InvalidateMeasure(_indicatorView);
		}

		public void UpdatePosition()
		{
			if (!UseDefaultIndicator)
				return;

			UpdateIndicatorColor(_currentPoistion, _indicatorView.IndicatorColor);
			_currentPoistion = _indicatorView.Position;
			UpdateIndicatorColor(_currentPoistion, _indicatorView.SelectedIndicatorColor);
		}

		public void UpdateCount()
		{
			if (!UseDefaultIndicator || _contentView == null)
				return;

			var count = Math.Min(_indicatorView.Count, _indicatorView.MaximumVisible);
			var diff = _indicators.Count - count;
			var needIncrease = diff < 0;

			diff = Math.Abs(diff);
			for (int i = 0; i < diff; i++)
			{
				if (needIncrease)
					IncreaseIndicator();
				else
					DecreaseIndicator();
			}
			UpdatePosition();
		}

		public TSize Measure(double availableWidth, double availableHeight)
		{
			if (UseDefaultIndicator)
			{
				return new TSize(IndicatorSizeWithMargin.ToScaledPixel() * _indicators.Count, IndicatorSizeWithMargin.ToScaledPixel());
			}
			else
			{
				return _templatedView?.CrossPlatformMeasure(availableWidth.ToScaledDP(), availableHeight.ToScaledDP()).ToPixel() ?? new TSize(0, 0);
			}
		}

		void UpdateIndicatorColor(int position, Paint? color)
		{
			if (position < 0 || position >= _indicators.Count)
				return;

			_indicators[position].Drawable.Background = color;
			_indicators[position].Invalidate();
		}

		void CreateTemplatedView()
		{
			UseDefaultIndicator = false;
			var layout = (_indicatorView as ITemplatedIndicatorView)?.IndicatorsLayoutOverride;
			if (layout == null || _indicatorView?.Handler?.MauiContext == null)
				return;
			_contentView = layout.ToPlatform(_indicatorView.Handler.MauiContext);
			_templatedView = layout;
			_templatedViewHandler = layout.Handler as IPlatformViewHandler;

			_contentView.WidthSpecification = NLayoutParamPolicies.MatchParent;
			_contentView.HeightSpecification = NLayoutParamPolicies.MatchParent;
			Children.Add(_contentView);
		}

		void CreateDefaultView()
		{
			UseDefaultIndicator = true;
			_contentView = new NView
			{
				WidthSpecification = NLayoutParamPolicies.MatchParent,
				HeightSpecification = NLayoutParamPolicies.MatchParent,
				Layout = new NLinearLayout
				{
					LinearOrientation = NLinearLayout.Orientation.Horizontal,
				}
			};
			Children.Add(_contentView);

			_currentPoistion = _indicatorView.Position;

			if (_indicatorView.Count == 1 && _indicatorView.HideSingle)
				return;

			var count = Math.Min(_indicatorView.Count, _indicatorView.MaximumVisible);

			for (int i = 0; i < count; i++)
			{
				var indicator = CreateIndicator((i == _indicatorView.Position) ? _indicatorView.SelectedIndicatorColor : _indicatorView.IndicatorColor);
				_contentView.Add(indicator);
				_indicators.Add(indicator);
			}
		}

		Indicator CreateIndicator(Paint? color)
		{
			var indicator = new Indicator()
			{
				Margin = new NExtents((ushort)DefaultMargin.ToScaledPixel()),
				Size = new NSize(IndicatorSize.ToScaledPixel(), IndicatorSize.ToScaledPixel())
			};
			indicator.Drawable.Shape = _indicatorView.IndicatorsShape;
			indicator.Drawable.Background = color;
			indicator.TouchEvent += OnIndicatorTouch;

			return indicator;
		}

		void ClearIndicatorView()
		{
			Children.Clear();

			if (_templatedViewHandler != null)
			{
				_templatedViewHandler.Dispose();
				_templatedViewHandler = null;
				_templatedView = null;
			}
			else
			{
				_contentView?.Dispose();
			}
			_contentView = null;

			foreach (var view in _indicators)
			{
				view.Dispose();
			}
			_indicators.Clear();
		}

		void DecreaseIndicator()
		{
			if (_contentView == null)
				return;

			var indicator = _indicators[_indicators.Count - 1];
			_contentView.Remove(indicator);
			_indicators.Remove(indicator);
			indicator.Dispose();
		}

		void IncreaseIndicator()
		{
			if (_contentView == null)
				return;

			var indicator = CreateIndicator(_indicators.Count == _indicatorView.Position ? _indicatorView.SelectedIndicatorColor : _indicatorView.IndicatorColor);
			_contentView.Add(indicator);
			_indicators.Add(indicator);
		}

		bool OnIndicatorTouch(object source, TouchEventArgs e)
		{
			if (e.Touch.GetState(0) == NPointStateType.Up && source is Indicator indicator)
			{
				var touchPosition = e.Touch.GetLocalPosition(0);
				if (0 < touchPosition.X && touchPosition.X < indicator.SizeWidth
					&& 0 < touchPosition.Y && touchPosition.Y < indicator.SizeHeight)
				{
					var position = _indicators.IndexOf(indicator);
					if (position != -1)
					{
						_indicatorView.Position = position;
					}
				}
			}
			return true;
		}

		void OnLayoutUpdated(object? sender, LayoutEventArgs e)
		{
			if (UseDefaultIndicator || _templatedView == null)
				return;

			var platformGeometry = this.GetBounds().ToDP();
			_templatedView.CrossPlatformMeasure(platformGeometry.Width, platformGeometry.Height);

			platformGeometry.X = 0;
			platformGeometry.Y = 0;
			_templatedView.CrossPlatformArrange(platformGeometry);
		}

		class Indicator : SkiaGraphicsView
		{
			public Indicator() : base(new MauiDrawable()) { }

			public new MauiDrawable Drawable => (MauiDrawable)base.Drawable!;
		}
	}
}