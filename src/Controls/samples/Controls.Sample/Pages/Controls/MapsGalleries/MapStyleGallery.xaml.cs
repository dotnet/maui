using Microsoft.Maui.Controls.Maps;

namespace Maui.Controls.Sample.Pages.MapsGalleries
{
	public partial class MapStyleGallery : ContentPage
	{
		// Dark mode style - hides landmarks, dark colors
		const string DarkStyle = """
[
  {"elementType":"geometry","stylers":[{"color":"#212121"}]},
  {"elementType":"labels.text.fill","stylers":[{"color":"#757575"}]},
  {"elementType":"labels.text.stroke","stylers":[{"color":"#212121"}]},
  {"featureType":"road","elementType":"geometry","stylers":[{"color":"#383838"}]},
  {"featureType":"road","elementType":"labels.text.fill","stylers":[{"color":"#9e9e9e"}]},
  {"featureType":"water","elementType":"geometry","stylers":[{"color":"#000000"}]},
  {"featureType":"water","elementType":"labels.text.fill","stylers":[{"color":"#3d3d3d"}]}
]
""";

		// Retro style - muted colors, vintage feel
		const string RetroStyle = """
[
  {"elementType":"geometry","stylers":[{"color":"#ebe3cd"}]},
  {"elementType":"labels.text.fill","stylers":[{"color":"#523735"}]},
  {"elementType":"labels.text.stroke","stylers":[{"color":"#f5f1e6"}]},
  {"featureType":"road","elementType":"geometry","stylers":[{"color":"#f5f1e6"}]},
  {"featureType":"road.highway","elementType":"geometry","stylers":[{"color":"#f8c967"}]},
  {"featureType":"road.highway","elementType":"geometry.stroke","stylers":[{"color":"#e9bc62"}]},
  {"featureType":"water","elementType":"geometry.fill","stylers":[{"color":"#b9d3c2"}]}
]
""";

		// Night style - dark blue theme
		const string NightStyle = """
[
  {"elementType":"geometry","stylers":[{"color":"#242f3e"}]},
  {"elementType":"labels.text.fill","stylers":[{"color":"#746855"}]},
  {"elementType":"labels.text.stroke","stylers":[{"color":"#242f3e"}]},
  {"featureType":"road","elementType":"geometry","stylers":[{"color":"#38414e"}]},
  {"featureType":"road","elementType":"geometry.stroke","stylers":[{"color":"#212a37"}]},
  {"featureType":"road","elementType":"labels.text.fill","stylers":[{"color":"#9ca5b3"}]},
  {"featureType":"road.highway","elementType":"geometry","stylers":[{"color":"#746855"}]},
  {"featureType":"road.highway","elementType":"geometry.stroke","stylers":[{"color":"#1f2835"}]},
  {"featureType":"road.highway","elementType":"labels.text.fill","stylers":[{"color":"#f3d19c"}]},
  {"featureType":"water","elementType":"geometry","stylers":[{"color":"#17263c"}]},
  {"featureType":"water","elementType":"labels.text.fill","stylers":[{"color":"#515c6d"}]}
]
""";

		public MapStyleGallery()
		{
			InitializeComponent();
		}

		void OnDefaultClicked(object? sender, EventArgs e)
		{
#pragma warning disable CA1416 // Validate platform compatibility - MapStyle only works on Android
			map.MapStyle = null;
#pragma warning restore CA1416
		}

		void OnDarkClicked(object? sender, EventArgs e)
		{
#pragma warning disable CA1416
			map.MapStyle = DarkStyle;
#pragma warning restore CA1416
		}

		void OnRetroClicked(object? sender, EventArgs e)
		{
#pragma warning disable CA1416
			map.MapStyle = RetroStyle;
#pragma warning restore CA1416
		}

		void OnNightClicked(object? sender, EventArgs e)
		{
#pragma warning disable CA1416
			map.MapStyle = NightStyle;
#pragma warning restore CA1416
		}
	}
}
