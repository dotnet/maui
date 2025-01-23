using UIKit;

namespace Maui.Controls.Sample
{
	public partial class IsCursorVisibleBehavior : PlatformBehavior<Entry, UITextField>
	{
		protected override void OnAttachedTo(Entry bindable, UITextField platformView)
		{
			base.OnAttachedTo(bindable, platformView);

			if (bindable is null)
				return;

			if (IsCursorVisible == false)
				HideCursor(platformView);
			else
				ShowCursor(platformView);
		}

		protected override void OnDetachedFrom(Entry bindable, UITextField platformView)
		{
			base.OnDetachedFrom(bindable, platformView);

			if (bindable is null)
				return;

			HideCursor(platformView);
		}

		void ShowCursor(UITextField textField)
		{
			textField.TintColor = UITextField.Appearance.TintColor;
		}

		void HideCursor(UITextField textField)
		{
			textField.TintColor = UIColor.Clear;
		}
	}
}
