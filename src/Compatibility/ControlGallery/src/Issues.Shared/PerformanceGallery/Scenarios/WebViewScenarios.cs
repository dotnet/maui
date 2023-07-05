using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class WebViewScenario1 : PerformanceScenario
	{
		public WebViewScenario1()
		: base("[WebView] Empty")
		{
			View = new WebView();
		}
	}
}
