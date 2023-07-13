using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class SwitchPage
	{
		public SwitchPage()
		{
			InitializeComponent();
		}

		private void Switch_Toggled(object sender, Microsoft.Maui.Controls.ToggledEventArgs e)
		{
			statusLabelSwitch.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().SetShowStatusLabel(e.Value);
		}
	}
}