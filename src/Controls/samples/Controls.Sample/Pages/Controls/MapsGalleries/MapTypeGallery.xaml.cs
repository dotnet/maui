using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages.MapsGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapTypeGallery
	{
		public MapTypeGallery()
		{
			InitializeComponent();
		}

		private void MapTypePicker_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			var picker = (Picker)sender;

			switch (picker.SelectedItem.ToString())
			{
				default:
				case "Street":
					mapTypeMap.MapType = Microsoft.Maui.MapType.Street;
					break;
				case "Satellite":
					mapTypeMap.MapType = Microsoft.Maui.MapType.Satellite;
					break;
				case "Hybrid":
					mapTypeMap.MapType = Microsoft.Maui.MapType.Hybrid;
					break;
			}
		}
	}
}