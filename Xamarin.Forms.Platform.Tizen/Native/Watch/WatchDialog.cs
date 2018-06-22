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
			Style = "circle";

			_popupLayout = new ELayout(this)
			{
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY = 1
			};

			_hasAcceptButton = hasAcceptButton;

			var style = hasAcceptButton ? "content/circle/buttons2" : "content/circle";

			_popupLayout.SetTheme("layout", "popup", style);
			_popupLayout.Show();

			SetContent(_popupLayout);
		}

		protected override void ApplyButton(ButtonPosition position, EButton button)
		{
			string style = "";
			string part = "";
			EColor color = EColor.Default;

			switch (position)
			{
				case ButtonPosition.Neutral:
					style = "popup/circle/right_check";
					part = "button2";
					break;

				case ButtonPosition.Negative:
					style = _hasAcceptButton ? "popup/circle/left_delete" : "bottom";
					color = _hasAcceptButton ? EColor.Default : new EColor(0, 47, 66, 255);
					part = "button1";
					break;

				case ButtonPosition.Positive:
				default:
					// Due to ux limiation, nothing to do
					break;
			}

			if (button != null)
			{
				button.Style = style;
				button.BackgroundColor = color;
			}
			SetPartContent(part, button);
		}

		protected override void ApplyContent(EvasObject content)
		{
			_popupLayout.SetContent(content);
		}

		protected override void ApplyTitle(string title)
		{
			_popupLayout.SetPartText("elm.text.title", title);
		}

		protected override void ApplyMessage(string message)
		{
			_popupLayout.SetPartText("elm.text", message);
		}
	}
}
