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
	public partial class DualViewMapPage : ContentPage
	{
		public DualViewMapPage()
		{
			InitializeComponent();
		}

		public void UpdateMap(MapItem item)
			=> map.UpdateMap(item);
	}
}