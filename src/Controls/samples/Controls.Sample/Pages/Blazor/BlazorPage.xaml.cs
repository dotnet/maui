using Maui.Controls.Sample.Pages.Base;

#if NET6_0_OR_GREATER
using Maui.Controls.Sample.Pages.Blazor;
using Microsoft.AspNetCore.Components.Web;
#endif

namespace Maui.Controls.Sample.Pages
{
	public partial class BlazorPage : BasePage
	{
		public BlazorPage()
		{
			InitializeComponent();

#if NET6_0_OR_GREATER
			bwv.RootComponents.RegisterForJavaScript<MyDynamicComponent>("my-dynamic-root-component");
#endif
		}
	}
}
