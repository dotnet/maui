using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui
{
	internal class CvCell : UICollectionViewCell
	{
		public VirtualListViewHandler Handler { get; set; }

		public CvViewContainer Container { get; private set; }

		public NSIndexPath IndexPath { get; set; }

		public PositionInfo PositionInfo { get; private set; }

		public IMauiContext Context { get; set; }

		[Export("initWithFrame:")]
		public CvCell(CGRect frame) : base(frame)
		{
		}

		public void Init(IMauiContext context)
		{
			if (Container == null)
			{
				Container = new CvViewContainer(context)
				{
					Frame = ContentView.Frame,
					AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth
				};
				ContentView.AddSubview(Container);
			}
		}

		public override UICollectionViewLayoutAttributes PreferredLayoutAttributesFittingAttributes(UICollectionViewLayoutAttributes layoutAttributes)
		{
			var attr = base.PreferredLayoutAttributesFittingAttributes(layoutAttributes);

			if (Container == null)
				return attr;

			Container.VirtualView.InvalidateMeasure();

			if (Handler.VirtualView.Orientation == ListOrientation.Vertical)
			{
				var virtSize = Container.VirtualView.Measure(attr.Frame.Width, double.MaxValue - 100);

				attr.Frame = new CGRect(0, attr.Frame.Y, attr.Frame.Width, virtSize.Height);
			}
			else
			{
				var virtSize = Container.VirtualView.Measure(double.MaxValue - 100, attr.Frame.Height);

				attr.Frame = new CGRect(0, attr.Frame.Y, virtSize.Width, attr.Frame.Height);
			}

			return attr;
		}

		public void Update(PositionInfo info)
		{
			PositionInfo = info;
			if (Container.VirtualView is IPositionInfo positionInfoView)
				positionInfoView.SetPositionInfo(info);

			Container.SetContainerNeedsLayout();
		}

		public void SwapView(IView view)
			=> Container.SwapView(view);

		public bool NeedsView
			=> Container?.NativeView == null;

		public IView VirtualView
			=> Container?.VirtualView;
	}
}