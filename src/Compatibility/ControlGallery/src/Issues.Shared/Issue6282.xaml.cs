using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6282, "Text on button loses center alignment on changing of IsEnabled", PlatformAffected.Android)]
	public partial class Issue6282 : ContentPage
	{
		public Issue6282()
		{
#if APP
			InitializeComponent();
			Task.Run(async () =>
			{
				await Task.Delay(1000);
				SwitchIsEnabled();
				await Task.Delay(1500);
				SwitchIsEnabled();
				await Task.Delay(1000);
				SwitchIsEnabled();
			});
#endif
		}

#if APP
		void SwitchIsEnabled() => Device.BeginInvokeOnMainThread(() => button.IsEnabled = buttonMaterial.IsEnabled = !button.IsEnabled);
#endif
	}
}
