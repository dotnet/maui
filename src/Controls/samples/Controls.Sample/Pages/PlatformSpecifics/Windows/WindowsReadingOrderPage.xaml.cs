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

        void OnToggleButtonClicked(object sender, EventArgs e)
        {
            _editor.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().SetDetectReadingOrderFromContent(!_editor.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().GetDetectReadingOrderFromContent());
            UpdateLabel();
        }

        void UpdateLabel()
        {
            _label.Text = $"FlowDirection: {_editor.FlowDirection}, DetectReadingOrderFromContent: {_editor.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().GetDetectReadingOrderFromContent()}";
        }
    }
}
