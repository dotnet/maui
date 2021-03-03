using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

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