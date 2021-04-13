using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Windows;
using Microsoft.Maui.Controls.Compatibility.ControlGallery;

[assembly: Dependency(typeof(PlatformSpecificCoreGalleryFactory))]

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Windows
{
	public class PlatformSpecificCoreGalleryFactory : IPlatformSpecificCoreGalleryFactory
	{
		public string Title => "Windows Core Gallery";

		public IEnumerable<(Func<Page> Create, string Title)> GetPages() => null;
	}
}
