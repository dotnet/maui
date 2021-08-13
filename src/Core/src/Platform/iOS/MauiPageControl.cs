using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform.iOS
{
	public class MauiPageControl : UIPageControl
	{
		const int DefaultIndicatorSize = 6;

		public MauiPageControl()
		{
			ValueChanged += MauiPageControlValueChanged;
		}

		public bool IsSquare { get; set; }

		public double IndicatorSize { get; set; }

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

		void UpdateSquareShape()
		{
			if (!NativeVersion.IsAtLeast(14))
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
						imageview.Image = UIImage.GetSystemImage("squareshape.fill");
						var frame = imageview.Frame;
						//the scare shape is not the same size as the circle so we might need to correct the frame
						imageview.Frame = new CGRect(frame.X - 6, frame.Y, frame.Width, frame.Height);
					}
				}
			}
		}

		void UpdateIndicatorSize()
		{
			if (IndicatorSize != DefaultIndicatorSize)
			{
				float scale = (float)IndicatorSize / DefaultIndicatorSize;
				var newTransform = CGAffineTransform.MakeScale(scale, scale);

				Transform = newTransform;
			}
		}

		void UpdateCornerRadius()
		{
			foreach (var view in Subviews)
			{
				view.Layer.CornerRadius = 0;
			}
		}

		void MauiPageControlValueChanged(object sender, System.EventArgs e)
		{
			//if we are iOS13 or lower and we are using a Square shape
			//we need to update the CornerRadius of the new shape.
			if (IsSquare && !NativeVersion.IsAtLeast(14))
			{
				LayoutSubviews();
			}
		}
	}
}
