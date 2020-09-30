using System;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
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
