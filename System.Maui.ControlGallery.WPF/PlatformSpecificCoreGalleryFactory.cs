using System;
using System.Collections.Generic;
using System.Maui;
using System.Maui.ControlGallery.WPF;
using System.Maui.Controls;

[assembly: Dependency(typeof(PlatformSpecificCoreGalleryFactory))]

namespace System.Maui.ControlGallery.WPF
{
	public class PlatformSpecificCoreGalleryFactory : IPlatformSpecificCoreGalleryFactory
	{
		public string Title => "WPF Core Gallery";

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
