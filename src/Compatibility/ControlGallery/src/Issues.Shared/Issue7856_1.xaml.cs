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
