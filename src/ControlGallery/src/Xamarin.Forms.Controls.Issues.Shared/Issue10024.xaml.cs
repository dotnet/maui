using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 10024, "Frame animation issue/inconsistency in latest version of Forms/Xamarin iOS", PlatformAffected.iOS)]
	public partial class Issue10024 : TestContentPage
	{
		public Issue10024()
		{
#if APP
			InitializeComponent();
#endif
		}

#if APP
		async Task ShowInformationFrame()
		{
			if (InformationFrame.IsVisible)
				return;

			InformationFrame.IsVisible = true;
			OpaqueContainer.IsVisible = true;
			InformationFrame.RotateYTo(-90, 0);
			await InformationFrame.TranslateTo(-200, 0, 0);

			OpaqueContainer.FadeTo(0.5, 250);
			InformationFrame.FadeTo(1, 250);
			InformationFrame.RotateYTo(0, 250, Easing.CubicOut);
			await InformationFrame.TranslateTo(0, 0, 250, Easing.CubicOut);
		}

		async void HideInformationFrame(object sender, System.EventArgs e)
		{
			OpaqueContainer.FadeTo(0, 250);
			InformationFrame.TranslateTo(200, 0, 250, Easing.CubicIn);
			InformationFrame.RotateYTo(90, 250, Easing.CubicIn);
			await InformationFrame.FadeTo(0, 250);

			InformationFrame.IsVisible = false;
			OpaqueContainer.IsVisible = false;
		}

		async void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
		{
			await ShowInformationFrame();
		}
#endif

		protected override void Init()
		{
		}
	}
}