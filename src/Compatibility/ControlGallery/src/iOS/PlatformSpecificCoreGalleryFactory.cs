using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.ControlGallery.iOS;
using Microsoft.Maui.Controls.Internals;

[assembly: Dependency(typeof(PlatformSpecificCoreGalleryFactory))]

namespace Microsoft.Maui.Controls.ControlGallery.iOS
{
	[Preserve(AllMembers = true)]
	public class PlatformSpecificCoreGalleryFactory : IPlatformSpecificCoreGalleryFactory
	{
		public string Title => "iOS Core Gallery";

		public IEnumerable<(Func<Page> Create, string Title)> GetPages()
		{
			return null;
		}
	}
}
