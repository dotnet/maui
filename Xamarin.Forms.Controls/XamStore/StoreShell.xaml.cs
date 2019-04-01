using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.XamStore
{
	[Preserve(AllMembers = true)]
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class StoreShell : TestShell
	{
		public StoreShell() 
		{
			InitializeComponent();
		}

		protected override void Init()
		{
			var fontFamily = "";
			switch (Device.RuntimePlatform)
			{
				case Device.iOS:
					fontFamily = "Ionicons";
					break;
				case Device.UWP:
					fontFamily = "Assets/Fonts/ionicons.ttf#ionicons";
					break;
				case Device.Android:
				default:
					fontFamily = "fonts/ionicons.ttf#";
					break;
			}
			FlyoutIcon = new FontImageSource
			{
				Glyph = "\uf2fb",
				FontFamily = fontFamily,
				Size = 20
			};
			CurrentItem = _storeItem;
			Routing.RegisterRoute("demo", typeof(DemoShellPage));
			Routing.RegisterRoute("demo/demo", typeof(DemoShellPage));
		}



		//bool allow = false;

		//protected override void OnNavigating(ShellNavigatingEventArgs args)
		//{
		//	if (allow)
		//		args.Cancel();

		//	allow = !allow;
		//	base.OnNavigating(args);
		//}
	}
}