using UIKit;

namespace System.Maui.Platform
{
	public class PickerView : UILabel
	{
		private UIView _inputView;
		private UIView _inputAccessoryView;

		public PickerView() 
		{
			UserInteractionEnabled = true;
			UITapGestureRecognizer tapGesture = new UITapGestureRecognizer(() => BecomeFirstResponder());
			AddGestureRecognizer(tapGesture);
		}

		public void SetInputView(UIView inputView) 
		{
			_inputView = inputView;
		}

		public void SetInputAccessoryView(UIView inputAccessoryView)
		{
			_inputAccessoryView = inputAccessoryView;
		}

		public override UIView InputView => _inputView ?? base.InputView;
		public override UIView InputAccessoryView => _inputAccessoryView ?? base.InputAccessoryView;

		public override bool CanBecomeFirstResponder => true;
	}
}
