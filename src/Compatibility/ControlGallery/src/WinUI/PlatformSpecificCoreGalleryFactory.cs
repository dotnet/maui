using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI;

[assembly: Dependency(typeof(PlatformSpecificCoreGalleryFactory))]

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI
{
	public class PlatformSpecificCoreGalleryFactory : IPlatformSpecificCoreGalleryFactory
	{
		public string Title => "Windows Core Gallery";

		public IEnumerable<(Func<Page> Create, string Title)> GetPages() => null;
	}
}
