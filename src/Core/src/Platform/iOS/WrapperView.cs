using UIKit;

namespace Microsoft.Maui
{
	public partial class WrapperView : UIView
	{
		UIView? _mainView;

		public WrapperView()
		{
			AutosizesSubviews = true;
		}

		public UIView? MainView
		{
			get => _mainView;
			set
			{
				if (_mainView == value)
					return;

				if (_mainView != null)
					_mainView.RemoveFromSuperview();

				_mainView = value;

				if (_mainView == null)
					return;

				Frame = _mainView.Frame;
				var oldParent = _mainView.Superview;

				if (oldParent != null)
					oldParent.InsertSubviewAbove(this, _mainView);

				_mainView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
				_mainView.Frame = Bounds;

				AddSubview(_mainView);
			}
		}

		public override void SizeToFit()
		{
			if (MainView == null)
				return;

			MainView.SizeToFit();
			Bounds = MainView.Bounds;

			base.SizeToFit();
		}
	}
}