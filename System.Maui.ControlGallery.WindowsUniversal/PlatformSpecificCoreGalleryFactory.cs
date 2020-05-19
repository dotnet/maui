using System;
using System.Collections.Generic;
using System.Maui;
using System.Maui.ControlGallery.WindowsUniversal;
using System.Maui.Controls;

[assembly: Dependency(typeof(PlatformSpecificCoreGalleryFactory))]

namespace System.Maui.ControlGallery.WindowsUniversal
{
	public class PlatformSpecificCoreGalleryFactory : IPlatformSpecificCoreGalleryFactory
	{
		public string Title => "Windows Core Gallery";

		public IEnumerable<(Func<Page> Create, string Title)> GetPages() => null;
	}
}
