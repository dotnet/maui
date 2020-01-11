using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.iOS;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Internals;

[assembly: Dependency(typeof(PlatformSpecificCoreGalleryFactory))]

namespace Xamarin.Forms.ControlGallery.iOS
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
