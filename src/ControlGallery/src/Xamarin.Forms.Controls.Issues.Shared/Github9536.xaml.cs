using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.Issues
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