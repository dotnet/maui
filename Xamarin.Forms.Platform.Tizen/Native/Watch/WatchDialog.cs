using ElmSharp;
using EButton = ElmSharp.Button;
using ELayout = ElmSharp.Layout;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen.Native.Watch
{
	public class WatchDialog : Dialog
	{
		ELayout _popupLayout;
		bool _hasAcceptButton = false ;

		/// <summary>
		/// Creates a dialog window for watch
		/// </summary>
		public WatchDialog(EvasObject parent, bool hasAcceptButton) : base(parent)
		{
			this.SetWatchCircleStyle();
			_hasAcceptButton = hasAcceptButton;
			_popupLayout = new PopupLayout(this, hasAcceptButton ? PopupLayout.Styles.Buttons2 : PopupLayout.Styles.Circle)
			{
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY = 1
			};
			_popupLayout.Show();
			SetContent(_popupLayout);
		}

		protected override void ApplyButton(ButtonPosition position, EButton button)
		{

			switch (position)
			{
				case ButtonPosition.Neutral:
					this.SetButton2Part(button.SetWatchPopupRightStyle());
					break;

				case ButtonPosition.Negative:
					if (_hasAcceptButton)
					{
						button.BackgroundColor = EColor.Default;
						this.SetButton1Part(button.SetWatchPopupLeftStyle());
					}
					else
					{
						button.BackgroundColor = new EColor(0, 47, 66, 255);
						this.SetButton1Part(button.SetBottomStyle());
					}
					break;

				case ButtonPosition.Positive:
				default:
					// Due to ux limiation, nothing to do
					break;
			}
		}

		protected override void ApplyContent(EvasObject content)
		{
			_popupLayout.SetContent(content);
		}

		protected override void ApplyTitle(string title)
		{
			if (_popupLayout is PopupLayout layout)
			{
				layout.SetTitleText(title);
			}
		}

		protected override void ApplyTitleColor(EColor color)
		{
			if (_popupLayout is PopupLayout layout)
			{
				layout.SetTitleColor(color);
			}
		}

		protected override void ApplyMessage(string message)
		{
			_popupLayout.SetTextPart(message);
		}
	}
}
