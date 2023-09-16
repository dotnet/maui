using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.XamStore
{
	[Preserve(AllMembers = true)]
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class StoreShell : TestShell
	{
		public StoreShell()
		{
			InitializeComponent();

			CurrentItem = _storeItem;
		}

		protected override void Init()
		{
			var fontFamily = "";
			switch (Device.RuntimePlatform)
			{
				case Device.iOS:
					fontFamily = "Ionicons";
					break;
				case Device.WinUI:
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
				Size = 20,
				AutomationId = "shellIcon"
			};

			FlyoutIcon.SetAutomationPropertiesHelpText("This as Shell FlyoutIcon");
			FlyoutIcon.SetAutomationPropertiesName("SHELLMAINFLYOUTICON");
			Routing.RegisterRoute("demo", typeof(DemoShellPage));
			Routing.RegisterRoute("demo/demo", typeof(DemoShellPage));
		}

		bool _defernavigationWithAlert;
		private void OnToggleNavigatingDeferral(object sender, System.EventArgs e)
		{
			_defernavigationWithAlert = !_defernavigationWithAlert;
			FlyoutIsPresented = false;
		}

		protected override async void OnNavigating(ShellNavigatingEventArgs args)
		{
			base.OnNavigating(args);

			if (_defernavigationWithAlert)
			{
				var token = args.GetDeferral();

				var result = await DisplayActionSheet(
					"Are you sure?",
					"cancel",
					"destruction",
					"Yes", "No");

				if (result != "Yes")
					args.Cancel();

				token.Complete();
			}
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