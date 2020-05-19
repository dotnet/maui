using System;
using System.Collections.Generic;
using System.Maui;
using System.Maui.ControlGallery.MacOS;
using System.Maui.Controls;

[assembly: Dependency(typeof(PlatformSpecificCoreGalleryFactory))]

namespace System.Maui.ControlGallery.MacOS
{
	public class PlatformSpecificCoreGalleryFactory : IPlatformSpecificCoreGalleryFactory
	{
		public string Title => "macOS Core Gallery";

		public IEnumerable<(Func<Page> Create, string Title)> GetPages()
		{
#if HAVE_OPENTK
			yield return (() => new BasicOpenGLGallery(), "Basic OpenGL Gallery - Legacy");
			yield return (() => new AdvancedOpenGLGallery(), "Advanced OpenGL Gallery - Legacy");
#else
			return null;
#endif
		}
	}
}
