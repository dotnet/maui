using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.DualScreen;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.TwoPaneViewGalleries
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
			System.Diagnostics.Debug.WriteLine($"{Device.info.ScaledScreenSize} {thing.Bounds}");
		}
	}
}