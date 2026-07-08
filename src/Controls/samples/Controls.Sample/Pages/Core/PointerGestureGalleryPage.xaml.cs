using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class PointerGestureGalleryPage
	{
		Command _hoverCommand;

		public PointerGestureGalleryPage()
		{
			InitializeComponent();

			_hoverCommand = new Command<Color>(HandleHoverCommand);

			var colorfulHoverGesture = new PointerGestureRecognizer
			{
				PointerEnteredCommand = _hoverCommand,
				PointerEnteredCommandParameter = Colors.Green,
				PointerExitedCommand = _hoverCommand,
				PointerExitedCommandParameter = Colors.Black
			};
			colorfulHoverLabel.GestureRecognizers.Add(colorfulHoverGesture);
		}

		void PointerHoverStarted(object sender, PointerEventArgs e)
		{
			pgrLabel.Text = "Thanks for hovering me! Now press me!";
			pgrLabel.BackgroundColor = Colors.PaleGreen;
		}

		void PointerHoverEnded(object sender, PointerEventArgs e)
		{
			pgrLabel.Text = "Hover me again!";
			pgrPositionLabel.Text = "Hover above label to reveal pointer position again";
			pgrLabel.BackgroundColor = Colors.Transparent;
		}

		void PointerMoved(object sender, PointerEventArgs e)
		{
			pgrPositionLabel.Text = $"Pointer position is at: {e.GetPosition((View)sender)}";
			pgrPositionToWindow.Text = $"Pointer position inside window: {e.GetPosition(null)}";
			pgrPositionToThisLabel.Text = $"Pointer position relative to this label: {e.GetPosition(pgrPositionToThisLabel)}";
		}

		void PointerPressStarted(object sender, PointerEventArgs e)
		{
			pgrLabel.Text = "Thanks for pressing me! Now release me!";
			pgrLabel.BackgroundColor = Colors.SkyBlue;
		}

		void PointerPressEnded(object sender, PointerEventArgs e)
		{
			pgrLabel.Text = "Thanks for releasing me! Press me again or leave me!";
			pgrLabel.BackgroundColor = Colors.Yellow;
		}

		void HoverBegan(object sender, PointerEventArgs e)
		{
			hoverLabel.Text = "Thanks for hovering me!";
		}

		void HoverEnded(object sender, PointerEventArgs e)
		{
			hoverLabel.Text = "Hover me again!";
			positionLabel.Text = "Hover above label to reveal pointer position again";
		}

		void HoverMoved(object sender, PointerEventArgs e)
		{
			positionLabel.Text = $"Pointer position is at: {e.GetPosition((View)sender)}";
			positionToWindow.Text = $"Pointer position inside window: {e.GetPosition(null)}";
			positionToThisLabel.Text = $"Pointer position relative to this label: {e.GetPosition(positionToThisLabel)}";
		}

		void HandleHoverCommand(Color hoverColor)
		{
			colorfulHoverLabel.TextColor = hoverColor;
		}
	}
}