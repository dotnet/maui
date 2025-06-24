using Maui.Controls.Sample.Pages.Base;
using Maui.Controls.Sample.Pages.Blazor;
using Microsoft.AspNetCore.Components.Web;
#if IOS || MACCATALYST
using Microsoft.AspNetCore.Components.WebView.Maui.PlatformConfiguration.iOSSpecific;
#endif

namespace Maui.Controls.Sample.Pages
{
	public partial class BlazorPage : BasePage
	{
		public BlazorPage()
		{
			InitializeComponent();

			bwv.RootComponents.RegisterForJavaScript<MyDynamicComponent>("my-dynamic-root-component");

#if IOS || MACCATALYST
			// Disable scroll bounce on iOS to make the app feel more native
			bwv.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>().SetIsScrollBounceEnabled(false);
#endif
		}
	}
}
