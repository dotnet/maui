using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Android;
using Microsoft.Maui.Controls.Internals;

[assembly: Dependency(typeof(PlatformSpecificCoreGalleryFactory))]

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Android
{
	[Preserve(AllMembers = true)]
	public class PlatformSpecificCoreGalleryFactory : IPlatformSpecificCoreGalleryFactory
	{
		public string Title => "Android Core Gallery";

		public IEnumerable<(Func<Page> Create, string Title)> GetPages()
		{
			return null;
		}
	}
}
