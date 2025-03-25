using Maui.Controls.Sample.Pages.Base;
using Maui.Controls.Sample.Pages.Blazor;
using Microsoft.AspNetCore.Components.Web;

namespace Maui.Controls.Sample.Pages
{
	public partial class BlazorPage : BasePage
	{
		public BlazorPage()
		{
			InitializeComponent();

			bwv.RootComponents.RegisterForJavaScript<MyDynamicComponent>("my-dynamic-root-component");
		}
	}
}
