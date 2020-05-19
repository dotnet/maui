using Tizen.NET.MaterialComponents;
using System.Maui.Material.Tizen;

namespace System.Maui
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