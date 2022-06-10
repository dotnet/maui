using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages.MapsGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPinsGallery
	{
		public MapPinsGallery()
		{
			InitializeComponent();

			pinsMap.Pins.Add(new Pin()
			{
				Label = "Test",
				Position = new Position(50.886475, 5.853191),
			});
		}
	}
}