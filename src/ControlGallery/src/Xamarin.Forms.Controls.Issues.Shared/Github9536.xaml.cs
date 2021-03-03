using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9536, "PopModalAsync hides navigation bar on macOS", PlatformAffected.macOS)]
	public partial class Github9536 : ContentPage
	{
		public Github9536()
		{
#if APP
			Title = "PopModalAsync";
			InitializeComponent();
#endif
		}

		void Button_Clicked(object sender, System.EventArgs e)
		{
			Navigation.PushModalAsync(new NavigationPage(new Issue2964.ModalPage()));
		}
	}
}