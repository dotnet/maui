using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Platform Specifics - iOS Translucent Navigation Bar XAML", PlatformAffected.iOS)]
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	public partial class PlatformSpecifics_iOSTranslucentNavBarX : TestNavigationPage
	{
		public PlatformSpecifics_iOSTranslucentNavBarX()
		{
#if APP
			InitializeComponent ();
#endif
		}

		protected override void Init()
		{
			var button = new Button { Text = "Toggle Translucent", BackgroundColor = Color.Yellow };

			button.Clicked += (sender, args) => On<iOS>().SetIsNavigationBarTranslucent(!On<iOS>().IsNavigationBarTranslucent());

			var content = new ContentPage
			{
				Title = "iOS Translucent Navigation Bar",
				Content = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					Children = { button }
				}
			};

			PushAsync(content);
		}
	}
}
