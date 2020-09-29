using System;
using System.Collections.Generic;
using Xamarin.Forms;

[assembly: ExportFont("CuteFont-Regular.ttf", Alias = "Foo")]
[assembly: ExportFont("PTM55FT.ttf")]
[assembly: ExportFont("Dokdo-Regular.ttf")]
[assembly: ExportFont("fa-regular-400.ttf", Alias = "FontAwesome")]

namespace Xamarin.Forms.Controls.GalleryPages
{
	public partial class EmbeddedFonts : ContentPage
	{
		public EmbeddedFonts()
		{
			InitializeComponent();
		}
	}
}
