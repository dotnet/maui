using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class ButtonPage
	{
		public ButtonPage()
		{
			InitializeComponent();
		}

		void OnPositionChange(object sender, System.EventArgs e)
		{
			var newPosition = ((int)positionChange.ContentLayout.Position) + 1;

			if (newPosition >= 4)
				newPosition = 0;

			positionChange.ContentLayout =
				new Button.ButtonContentLayout((Button.ButtonContentLayout.ImagePosition)newPosition,
					positionChange.ContentLayout.Spacing);
		}

		void OnDecreaseSpacing(object sender, System.EventArgs e)
		{
			positionChange.ContentLayout =
				new Button.ButtonContentLayout(positionChange.ContentLayout.Position,
					positionChange.ContentLayout.Spacing - 1);
		}

		void OnIncreasingSpacing(object sender, System.EventArgs e)
		{
			positionChange.ContentLayout =
				new Button.ButtonContentLayout(positionChange.ContentLayout.Position,
					positionChange.ContentLayout.Spacing + 1);
		}
	}
}