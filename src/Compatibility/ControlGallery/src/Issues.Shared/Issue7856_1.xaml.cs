//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using System.Collections.Generic;

using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	public partial class Issue7856_1 : ContentPage
	{
		public Issue7856_1()
		{
#if APP
			InitializeComponent();
#endif

			Shell.SetBackButtonBehavior(this, new BackButtonBehavior
			{
				TextOverride = "Test"
			});
		}

		private void Navigate_Clicked(object sender, EventArgs e)
		{
			_ = Shell.Current.Navigation.PushAsync(new Issue7856_1());
		}
	}
}
