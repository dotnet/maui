using System;
using System.Collections.Generic;
using System.Maui;

[assembly: ExportFont("CuteFont-Regular.ttf", Alias = "Foo")]
[assembly: ExportFont("PTM55FT.ttf")]
[assembly: ExportFont("Dokdo-Regular.ttf")]
[assembly: ExportFont("fa-regular-400.ttf")]

namespace System.Maui.Controls.GalleryPages
{
	public partial class EmbeddedFonts : ContentPage
	{
		public EmbeddedFonts()
		{
			InitializeComponent();
		}
	}
}
