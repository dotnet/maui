using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class BorderNoLayoutContent : ContentPage
	{
		public BorderNoLayoutContent()
		{
			InitializeComponent();
		}

		void OnContentBackgroundCheckBoxChanged(object sender, CheckedChangedEventArgs e)
		{
			UpdateContentBackground();
		}

		void UpdateContentBackground()
		{
			var contentBackground = ContentBackgroundCheckBox.IsChecked ? Color.FromArgb("#99FF0000") : Colors.Transparent;

			CircleLabel.BackgroundColor = PlayLabel.BackgroundColor = StopLabel.BackgroundColor = contentBackground;
		}
	}
}