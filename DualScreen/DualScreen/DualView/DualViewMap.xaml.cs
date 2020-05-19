using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Maui;
using System.Maui.Xaml;

namespace DualScreen
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DualViewMap : ContentView
	{
		public DualViewMap()
		{
			InitializeComponent();

			webView.Source = $"file:///android_asset/googlemap.html";
		}

		public void UpdateMap(MapItem item)
			=> webView.Source = $"file:///android_asset/googlemap.html?lat={item.Lat}&lng={item.Lng}";
	}
}