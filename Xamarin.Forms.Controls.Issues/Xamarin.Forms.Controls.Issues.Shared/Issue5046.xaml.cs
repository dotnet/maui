using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5046, "CornerRadius doesn't work in explicit style when implicit style exists",
		   PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.UWP)]
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	public partial class Issue5046 : ContentPage
	{
		public Issue5046()
		{
#if APP
			InitializeComponent();
#endif
		}

		void Button_Clicked(object sender, System.EventArgs e)
		{
#if APP
			this.DisplayAlert("CornerRadius", "CornerRadius = " + ((Button)sender).CornerRadius, "OK");
#endif
		}
	}
}