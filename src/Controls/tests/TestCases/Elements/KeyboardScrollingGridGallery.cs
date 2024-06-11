using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample;

[Preserve(AllMembers = true)]
public class KeyboardScrollingGridGallery : ContentViewGalleryPage
{
	public KeyboardScrollingGridGallery()
	{
		Content = new KeyboardScrollingGridPage();
	}
}
