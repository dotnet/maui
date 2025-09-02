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

		WeakReference<IIndicatorView>? _indicatorView;
		bool _updatingPosition;

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

			if (Subviews.Length == 0)
				return;

			UpdateIndicatorSize();

			if (!IsSquare)
				return;

			UpdateSquareShape();
		}

		public void UpdateIndicatorSize()
		{
			if (IndicatorSize == 0 || IndicatorSize == DefaultIndicatorSize)
				return;

			float scale = (float)IndicatorSize / DefaultIndicatorSize;
			var newTransform = CGAffineTransform.MakeScale(scale, scale);
			if (Subviews.Length > 0)
			{
				foreach (var view in Subviews)
				{
					view.Transform = newTransform;
				}
			}
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
			this.UpdatePages(indicatorView.GetMaximumVisible());
			UpdatePosition();
		}

		void UpdateSquareShape()
		{
			if (!(OperatingSystem.IsIOSVersionAtLeast(14) || OperatingSystem.IsTvOSVersionAtLeast(14)))
			{
				UpdateCornerRadius();
				return;
			}

			var uiPageControlContentView = Subviews[0];
			if (uiPageControlContentView.Subviews.Length > 0)
			{
				var uiPageControlIndicatorContentView = uiPageControlContentView.Subviews[0];

				foreach (var view in uiPageControlIndicatorContentView.Subviews)
				{
					if (view is UIImageView imageview)
					{
						if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13))
							imageview.Image = UIImage.GetSystemImage("squareshape.fill");
						var frame = imageview.Frame;
						//the square shape is not the same size as the circle so we might need to correct the frame
						imageview.Frame = new CGRect(frame.X - 6, frame.Y, frame.Width, frame.Height);
					}
				}
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
			if (IsSquare && !(OperatingSystem.IsIOSVersionAtLeast(14) || OperatingSystem.IsTvOSVersionAtLeast(14)))
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
