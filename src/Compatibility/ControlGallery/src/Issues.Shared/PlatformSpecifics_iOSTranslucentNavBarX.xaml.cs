using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
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
			InitializeComponent();
#endif
		}

		protected override void Init()
		{
			var button = new Button { Text = "Toggle Translucent", BackgroundColor = Colors.Yellow };

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
