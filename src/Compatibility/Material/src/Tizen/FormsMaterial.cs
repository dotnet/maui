using Tizen.NET.MaterialComponents;
using Microsoft.Maui.Controls.Compatibility.Material.Tizen;

namespace Microsoft.Maui.Controls
{
	public static class FormsMaterial
	{
		public static void Init()
		{
			// my only purpose is to exist so when called
			// this dll doesn't get removed

			VisualMarker.RegisterMaterial();
			MColors.Current = MaterialColors.Light.CreateColorScheme();
		}
	}
}