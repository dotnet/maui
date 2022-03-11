using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.DualScreen;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Essentials;

namespace DualScreen
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class NestedTwoPaneViewSplitAcrossHinge : ContentPage
	{
		public NestedTwoPaneViewSplitAcrossHinge()
		{
			InitializeComponent();
		}

		void TwoPaneView_LayoutChanged(object sender, EventArgs e)
		{
			var thing = (TwoPaneView)sender;
			System.Diagnostics.Debug.WriteLine($"{DeviceDisplay.MainDisplayInfo.Density} {thing.Bounds}");
		}
	}
}