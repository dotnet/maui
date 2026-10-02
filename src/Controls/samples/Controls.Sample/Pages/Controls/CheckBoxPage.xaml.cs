using System;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class CheckBoxPage
	{
		bool _isGreen;

		public CheckBoxPage()
		{
			InitializeComponent();
			UpdateControls();
		}

		void OnChangeIsCheckedButtonClicked(object? sender, EventArgs e)
		{
			_isGreen = !_isGreen;
			UpdateControls();
		}

		void UpdateControls()
		{
			IsCheckedCheckBox.Color = _isGreen ? Colors.Green : Colors.Red;
			ChangeIsCheckedButton.TextColor = IsCheckedCheckBox.Color;
			ChangeIsCheckedButton.Text = $"Is green? {_isGreen}";
		}
	}
}