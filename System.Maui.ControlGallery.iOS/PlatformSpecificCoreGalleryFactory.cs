using System;
using System.Collections.Generic;
using System.Maui;
using System.Maui.ControlGallery.iOS;
using System.Maui.Controls;
using System.Maui.Internals;

[assembly: Dependency(typeof(PlatformSpecificCoreGalleryFactory))]

namespace System.Maui.ControlGallery.iOS
{
	[Preserve(AllMembers = true)]
	public class PlatformSpecificCoreGalleryFactory : IPlatformSpecificCoreGalleryFactory
	{
		public string Title => "iOS Core Gallery";

		public IEnumerable<(Func<Page> Create, string Title)> GetPages()
		{
#if HAVE_OPENTK
			yield return (() => new AdvancedOpenGLGallery(), "Advanced OpenGL Gallery - Legacy");
#else
			return null;
#endif
		}
	}
}
