using System;
using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiPageControl : UIPageControl
	{
		const int DefaultIndicatorSize = 6;

		IIndicatorView? _indicatorView;
		bool _updatingPosition;

		public MauiPageControl()
		{
			ValueChanged += MauiPageControlValueChanged;

			if (OperatingSystem.IsIOSVersionAtLeast(14) || OperatingSystem.IsMacCatalystVersionAtLeast(14)
#if TVOS
				|| OperatingSystem.IsTvOSVersionAtLeast(14)
#endif
			)
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
			_indicatorView = indicatorView;

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

			Transform = newTransform;
		}

		public void UpdatePosition()
		{
			_updatingPosition = true;
			this.UpdateCurrentPage(GetCurrentPage());
			_updatingPosition = false;

			int GetCurrentPage()
			{
				if (_indicatorView == null)
					return -1;

				var maxVisible = _indicatorView.GetMaximumVisible();
				var position = _indicatorView.Position;
				var index = position >= maxVisible ? maxVisible - 1 : position;
				return index;
			}
		}

		public void UpdateIndicatorCount()
		{
			if (_indicatorView == null)
				return;
			this.UpdatePages(_indicatorView.GetMaximumVisible());
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
			if (_updatingPosition || _indicatorView == null)
				return;

			_indicatorView.Position = (int)CurrentPage;
			//if we are iOS13 or lower and we are using a Square shape
			//we need to update the CornerRadius of the new shape.
			if (IsSquare && !(OperatingSystem.IsIOSVersionAtLeast(14) || OperatingSystem.IsTvOSVersionAtLeast(14)))
				LayoutSubviews();

		}
	}
}
