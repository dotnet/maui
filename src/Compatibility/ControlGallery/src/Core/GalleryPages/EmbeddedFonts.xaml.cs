using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

[assembly: ExportFont("CuteFont-Regular.ttf", Alias = "Foo")]
[assembly: ExportFont("PTM55FT.ttf")]
[assembly: ExportFont("Dokdo-Regular.ttf")]
[assembly: ExportFont("fa-regular-400.ttf", Alias = "FontAwesome")]

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages
{
	public partial class EmbeddedFonts : ContentPage
	{
		public EmbeddedFonts()
		{
			InitializeComponent();
		}
	}
}
