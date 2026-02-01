using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class WindowsReadingOrderPage : ContentPage
	{
		public WindowsReadingOrderPage()
		{
			InitializeComponent();

			UpdateLabel();
		}

		void OnToggleButtonClicked(object? sender, EventArgs e)
		{
			var detectReadingOrder = _editor.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().GetDetectReadingOrderFromContent();

			_editor.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().SetDetectReadingOrderFromContent(!detectReadingOrder);
			_label.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().SetDetectReadingOrderFromContent(!detectReadingOrder);

			UpdateLabel();
		}

		void UpdateLabel()
		{
			var detectReadingOrder = _editor.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().GetDetectReadingOrderFromContent();
			_info.Text = $"FlowDirection: {_editor.FlowDirection}, DetectReadingOrderFromContent: {detectReadingOrder}";
		}
	}
}
