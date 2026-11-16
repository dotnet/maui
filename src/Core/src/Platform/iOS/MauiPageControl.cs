using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiPageControl : UIPageControl, IUIViewLifeCycleEvents
	{
		const int DefaultIndicatorSize = 6;
		const double IndicatorSizeTolerance = 0.001;
		const string SquareSymbol = "squareshape.fill";
		const string CircleSymbol = "circlebadge.fill";
		WeakReference<IIndicatorView>? _indicatorView;
		bool _updatingPosition;
		double _lastAppliedIndicatorSize = DefaultIndicatorSize;
		WeakReference<UIView>? _indicatorLayoutView;

		public MauiPageControl()
		{
			ValueChanged += MauiPageControlValueChanged;
			if (OperatingSystem.IsIOSVersionAtLeast(14) || OperatingSystem.IsMacCatalystVersionAtLeast(14) || OperatingSystem.IsTvOSVersionAtLeast(14))
			{
				AllowsContinuousInteraction = false;
				BackgroundStyle = UIPageControlBackgroundStyle.Minimal;
			}
		}

		public void SetIndicatorView(IIndicatorView? indicatorView)
		{
			if (indicatorView == null)
			{
				ValueChanged -= MauiPageControlValueChanged;
				if (_indicatorLayoutView?.TryGetTarget(out var indicatorLayoutView) == true)
					indicatorLayoutView.RemoveFromSuperview();
				_indicatorLayoutView = null;
			}
			_indicatorView = indicatorView is null ? null : new(indicatorView);

		}

		public bool IsSquare { get; set; }

		public double IndicatorSize { get; set; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				ValueChanged -= MauiPageControlValueChanged;

			base.Dispose(disposing);
		}


		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			IIndicatorView? indicatorView = null;
			_indicatorView?.TryGetTarget(out indicatorView);

			if ((indicatorView as ITemplatedIndicatorView)?.IndicatorsLayoutOverride is ILayout layout)
			{
				// The template host needs valid bounds before RTL child positioning runs.
				layout.Arrange(Bounds.ToRectangle());
				return;
			}

			if (Subviews.Length == 0)
				return;

			UpdateIndicatorSize();

			if (indicatorView != null)
			{
				UpdateIndicatorShape();
			}
		}

		/// <inheritdoc/>
		public override CGSize SizeThatFits(CGSize size)
		{
			if (_indicatorView?.TryGetTarget(out var indicatorView) == true
				&& (indicatorView as ITemplatedIndicatorView)?.IndicatorsLayoutOverride is ILayout layout)
			{
				return layout.Measure((double)size.Width, (double)size.Height).ToCGSize();
			}

			return base.SizeThatFits(size);
		}

		public void UpdateIndicatorSize()
		{
			if (_indicatorLayoutView != null)
				return;

			if (IndicatorSize == 0 || IndicatorSize == _lastAppliedIndicatorSize)
				return;

			if (Math.Abs(IndicatorSize - _lastAppliedIndicatorSize) < IndicatorSizeTolerance)
				return;

			float scale = (float)IndicatorSize / DefaultIndicatorSize;
			var newTransform = CGAffineTransform.MakeScale(scale, scale);
			foreach (var view in Subviews)
			{
				view.Transform = newTransform;
			}

			_lastAppliedIndicatorSize = IndicatorSize;
		}

		public void UpdatePosition()
		{
			_updatingPosition = true;
			this.UpdateCurrentPage(GetCurrentPage());
			_updatingPosition = false;

			int GetCurrentPage()
			{
				if (_indicatorView is null || !_indicatorView.TryGetTarget(out var indicatorView))
					return -1;

				var maxVisible = indicatorView.GetMaximumVisible();
				var position = indicatorView.Position;
				var index = position >= maxVisible ? maxVisible - 1 : position;
				return index;
			}
		}

		public void UpdateIndicatorCount()
		{
			if (_indicatorView is null || !_indicatorView.TryGetTarget(out var indicatorView))
				return;
			this.UpdatePages((indicatorView as ITemplatedIndicatorView)?.IndicatorsLayoutOverride == null
				? indicatorView.GetMaximumVisible() : 0);
			UpdatePosition();
		}

		internal void UpdateIndicatorTemplate(ILayout? layout, IMauiContext? mauiContext)
		{
			if (_indicatorLayoutView?.TryGetTarget(out var oldLayoutView) == true)
				oldLayoutView.RemoveFromSuperview();
			_indicatorLayoutView = null;

			if (layout != null)
			{
				if (mauiContext == null)
					throw new InvalidOperationException("MauiContext is required to display an indicator template.");

				var indicatorLayoutView = layout.ToPlatform(mauiContext);
				indicatorLayoutView.MarkAsCrossPlatformLayoutBacking();
				_indicatorLayoutView = new(indicatorLayoutView);
				AddSubview(indicatorLayoutView);
			}
			else
			{
				_lastAppliedIndicatorSize = double.NaN;
			}

			UpdateIndicatorCount();
			SetNeedsLayout();
		}

		void UpdateIndicatorShape()
		{
			if (!(OperatingSystem.IsIOSVersionAtLeast(14) || OperatingSystem.IsTvOSVersionAtLeast(14) || OperatingSystem.IsMacCatalystVersionAtLeast(14)))
			{
				UpdateCornerRadius();
				return;
			}

			var uiPageControlContentView = Subviews[0];
			if (uiPageControlContentView.Subviews.Length > 0)
			{
				foreach (var uiPageControlIndicatorContentView in uiPageControlContentView.Subviews)
				{
					SetIndicatorShape(uiPageControlIndicatorContentView, IsSquare);
				}
			}
		}

		// Recursively find UIImageView and set its image
		static void SetIndicatorShape(UIView view, bool isSquare)
		{
			if (view is UIImageView imageView)
			{
				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13))
				{
					imageView.Image = UIImage.GetSystemImage(isSquare ? SquareSymbol : CircleSymbol);
					return;
				}
			}

			if (view.Subviews is null || view.Subviews.Length == 0)
			{
				return;
			}

			foreach (var child in view.Subviews)
			{
				SetIndicatorShape(child, isSquare);
			}
		}

		void UpdateCornerRadius()
		{
			foreach (var view in Subviews)
			{
				view.Layer.CornerRadius = 0;
			}
		}

		void MauiPageControlValueChanged(object? sender, System.EventArgs e)
		{
			if (_updatingPosition || _indicatorView is null || !_indicatorView.TryGetTarget(out var indicatorView))
				return;

			indicatorView.Position = (int)CurrentPage;
			//if we are iOS13 or lower and we are using a Square shape
			//we need to update the CornerRadius of the new shape.
			if (IsSquare && !(OperatingSystem.IsIOSVersionAtLeast(14) || OperatingSystem.IsTvOSVersionAtLeast(14) || OperatingSystem.IsMacCatalystVersionAtLeast(14)))
				LayoutSubviews();

		}

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler? _movedToWindow;
		event EventHandler IUIViewLifeCycleEvents.MovedToWindow
		{
			add => _movedToWindow += value;
			remove => _movedToWindow -= value;
		}

		public override void MovedToWindow()
		{
			_movedToWindow?.Invoke(this, EventArgs.Empty);
		}
	}
}
